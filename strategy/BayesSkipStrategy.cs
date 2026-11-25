using System;
using System.Collections.Generic;

/// <summary>
/// Прогулы с байесовским выводом правил преподавателей.
///
/// У каждого преподавателя одно из трёх правил: спросить случайно (с вероятностью 1/2),
/// спросить, если вчера спросили по предмету A, или спросить, если вчера спросили ровно по одному
/// из предметов A и Б. Всего 22 гипотезы на преподавателя: 1 + 6 + 15.
/// По истории стратегия вычисляет вероятность каждой гипотезы, из неё вероятность того, что
/// предмет спросят сегодня, и прогуливает его, только если эта вероятность ниже порога.
///
/// Стратегия не хранит состояние: все вероятности пересчитываются по истории на каждом вызове,
/// поэтому один экземпляр можно безопасно использовать для любого числа семестров и потоков.
/// </summary>
public sealed class BayesSkipStrategy : ISkipStrategy
{
    private const int FirstDay = 1;
    private const int SubjectCount = 6;

    // Прогул даёт 1 очко, а вылет обнуляет весь семестр (около 350 очков), поэтому прогул выгоден,
    // только если вероятность вопроса меньше примерно 1/350. Значение подобрано симуляцией.
    private const double DefaultRiskThreshold = 0.003;

    private const double RandomRuleAskProbability = 0.5;
    private const int RandomRuleIndex = 0;
    private const double MinimumTotalProbability = 1e-300;

    private enum RuleKind
    {
        Random,
        CopyYesterday,
        XorYesterday
    }

    private readonly record struct Hypothesis(RuleKind Kind, int SubjectA, int SubjectB, double PriorProbability);

    private static readonly Hypothesis[] Hypotheses = CreateHypotheses();

    // AskedProbability[гипотеза, вчерашняя маска] = вероятность, что сегодня спросят.
    // Бит i маски равен 1, если вчера спросили по предмету i.
    private static readonly double[,] AskedProbability = CreateAskedProbabilityTable();

    private readonly double _riskThreshold;

    public BayesSkipStrategy() : this(DefaultRiskThreshold)
    {
    }

    public BayesSkipStrategy(double riskThreshold)
    {
        _riskThreshold = riskThreshold;
    }

    public string Name => "Bayes";

    public bool[] DecideDay(int day, IReadOnlyStudentHistory history)
    {
        double[][] beliefs = CreatePriorBeliefs();
        int askedYesterdayMask = 0;

        for (int pastDay = FirstDay; pastDay < day; pastDay++)
        {
            askedYesterdayMask = ApplyDayObservations(beliefs, history, pastDay, askedYesterdayMask);
        }

        var goToLesson = new bool[SubjectCount];

        for (int subject = 0; subject < SubjectCount; subject++)
        {
            double askedProbability = CalculateAskedProbability(beliefs[subject], askedYesterdayMask);
            goToLesson[subject] = askedProbability >= _riskThreshold;
        }

        return goToLesson;
    }

    private static Hypothesis[] CreateHypotheses()
    {
        const double probabilityPerRuleKind = 1.0 / 3.0;
        int pairCount = SubjectCount * (SubjectCount - 1) / 2;

        var hypotheses = new List<Hypothesis>
        {
            new Hypothesis(RuleKind.Random, -1, -1, probabilityPerRuleKind)
        };

        for (int a = 0; a < SubjectCount; a++)
        {
            hypotheses.Add(new Hypothesis(RuleKind.CopyYesterday, a, -1, probabilityPerRuleKind / SubjectCount));
        }

        for (int a = 0; a < SubjectCount; a++)
        {
            for (int b = a + 1; b < SubjectCount; b++)
            {
                hypotheses.Add(new Hypothesis(RuleKind.XorYesterday, a, b, probabilityPerRuleKind / pairCount));
            }
        }

        return hypotheses.ToArray();
    }

    private static double[,] CreateAskedProbabilityTable()
    {
        int maskCount = 1 << SubjectCount;
        var table = new double[Hypotheses.Length, maskCount];

        for (int hypothesis = 0; hypothesis < Hypotheses.Length; hypothesis++)
        {
            for (int mask = 0; mask < maskCount; mask++)
            {
                table[hypothesis, mask] = CalculateAskedProbability(Hypotheses[hypothesis], mask);
            }
        }

        return table;
    }

    private static double CalculateAskedProbability(Hypothesis hypothesis, int askedYesterdayMask)
    {
        return hypothesis.Kind switch
        {
            RuleKind.Random => RandomRuleAskProbability,
            RuleKind.CopyYesterday => WasAskedYesterday(askedYesterdayMask, hypothesis.SubjectA) ? 1.0 : 0.0,
            _ => WasAskedYesterday(askedYesterdayMask, hypothesis.SubjectA)
                 != WasAskedYesterday(askedYesterdayMask, hypothesis.SubjectB) ? 1.0 : 0.0
        };
    }

    private static bool WasAskedYesterday(int askedYesterdayMask, int subject)
    {
        return (askedYesterdayMask & (1 << subject)) != 0;
    }

    private static double[][] CreatePriorBeliefs()
    {
        var beliefs = new double[SubjectCount][];

        for (int subject = 0; subject < SubjectCount; subject++)
        {
            beliefs[subject] = new double[Hypotheses.Length];

            for (int hypothesis = 0; hypothesis < Hypotheses.Length; hypothesis++)
            {
                beliefs[subject][hypothesis] = Hypotheses[hypothesis].PriorProbability;
            }
        }

        return beliefs;
    }

    /// <summary>Учитывает исходы одного прошедшего дня и возвращает маску тех, кого в этот день спросили.</summary>
    private static int ApplyDayObservations(
        double[][] beliefs,
        IReadOnlyStudentHistory history,
        int day,
        int askedYesterdayMask)
    {
        int askedTodayMask = 0;

        for (int subject = 0; subject < SubjectCount; subject++)
        {
            // Если студент прогулял и дожил до сегодняшнего дня, значит, его не спросили.
            // Для прогулянной пары WasAsked может вернуть null, поэтому сравнение именно с true.
            bool wasAsked = history.WasAsked(day, (Subject)subject) == true;

            if (wasAsked)
            {
                askedTodayMask |= 1 << subject;
            }

            UpdateBelief(beliefs[subject], wasAsked, askedYesterdayMask);
        }

        return askedTodayMask;
    }

    private static void UpdateBelief(double[] belief, bool wasAsked, int askedYesterdayMask)
    {
        double total = 0;

        for (int hypothesis = 0; hypothesis < belief.Length; hypothesis++)
        {
            double askedProbability = AskedProbability[hypothesis, askedYesterdayMask];
            belief[hypothesis] *= wasAsked ? askedProbability : 1.0 - askedProbability;
            total += belief[hypothesis];
        }

        if (total <= MinimumTotalProbability)
        {
            // История противоречит всем гипотезам: считаем преподавателя случайным и ходим на его пары.
            Array.Clear(belief);
            belief[RandomRuleIndex] = 1.0;
            return;
        }

        for (int hypothesis = 0; hypothesis < belief.Length; hypothesis++)
        {
            belief[hypothesis] /= total;
        }
    }

    private static double CalculateAskedProbability(double[] belief, int askedYesterdayMask)
    {
        double probability = 0;

        for (int hypothesis = 0; hypothesis < belief.Length; hypothesis++)
        {
            probability += belief[hypothesis] * AskedProbability[hypothesis, askedYesterdayMask];
        }

        return probability;
    }
}
