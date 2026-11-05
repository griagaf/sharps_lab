using Lab5.Contracts;
using Lab5.Teacher.Randomness;

namespace Lab5.Teacher.Rules;

public class TeacherRuleGenerator
{
    private readonly IRandomSource _random;

    public TeacherRuleGenerator(IRandomSource random)
    {
        _random = random;
    }

    /// <summary>Одно из трёх правил, выбранное с равной вероятностью.</summary>
    public ITeacherRule CreateRandomRule()
    {
        int ruleNumber = _random.Next(3) + 1;

        switch (ruleNumber)
        {
            case 1:
                return new RandomRule(_random);

            case 2:
                return new CopyYesterdayRule(PickRandomSubject());

            default:
                Subject subjectA = PickRandomSubject();
                Subject subjectB = PickRandomSubject();

                while (subjectB == subjectA)
                {
                    subjectB = PickRandomSubject();
                }

                return new XorYesterdayRule(subjectA, subjectB);
        }
    }

    private Subject PickRandomSubject()
    {
        return Subjects.All[_random.Next(Subjects.Count)];
    }
}
