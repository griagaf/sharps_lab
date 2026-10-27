using Lab3.Domain;
using Lab3.Students;

namespace Lab3.Tests.Students;

public class EvenDaysStrategyTests
{
    private const int ObservationDays = 20;
    private const int FirstOddDayAfterObservation = ObservationDays + 1;

    private readonly EvenDaysStrategy _strategy = new EvenDaysStrategy(ObservationDays);

    // Студент был на всех парах с firstDay по lastDay, спрашивали только по askedSubjects.
    private static void AddDays(StudentHistory history, int firstDay, int lastDay, params Subject[] askedSubjects)
    {
        for (int day = firstDay; day <= lastDay; day++)
        {
            foreach (Subject subject in Subjects.All)
            {
                history.SaveLesson(day, subject, attended: true, asked: askedSubjects.Contains(subject));
            }
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(ObservationDays)]
    public void DuringObservation_AttendsEverything(int day)
    {
        var history = new StudentHistory();
        AddDays(history, 1, day - 1);

        Assert.Equal(Flags.All(), _strategy.DecideDay(day, history));
    }

    [Theory]
    [InlineData(ObservationDays + 2)]
    [InlineData(100)]
    [InlineData(1000)]
    public void EvenDayAfterObservation_AttendsEverything(int day)
    {
        var history = new StudentHistory();
        AddDays(history, 1, day - 1);

        Assert.Equal(Flags.All(), _strategy.DecideDay(day, history));
    }

    [Fact]
    public void OddDay_NeverAsked_SkipsEverything()
    {
        var history = new StudentHistory();
        AddDays(history, 1, ObservationDays);

        Assert.Equal(Flags.None(), _strategy.DecideDay(FirstOddDayAfterObservation, history));
    }

    [Fact]
    public void OddDay_AttendsOnlySubjectsThatWereEverAsked()
    {
        var history = new StudentHistory();
        AddDays(history, 1, 2);
        AddDays(history, 3, 3, Subject.Calculus);
        AddDays(history, 4, 10);
        AddDays(history, 11, 11, Subject.English, Subject.Calculus);
        AddDays(history, 12, 19);
        AddDays(history, 20, 20, Subject.Physics);

        bool[] decision = _strategy.DecideDay(FirstOddDayAfterObservation, history);

        Assert.Equal(Flags.Only(Subject.Calculus, Subject.Physics, Subject.English), decision);
    }

    [Fact]
    public void OddDay_AskedLongAgo_IsNotForgotten()
    {
        var history = new StudentHistory();
        AddDays(history, 1, 1, Subject.DiscreteMath);
        AddDays(history, 2, 998);

        Assert.Equal(Flags.Only(Subject.DiscreteMath), _strategy.DecideDay(999, history));
    }

    [Fact]
    public void OddDay_SkippedLesson_IsNotCountedAsAsked()
    {
        var history = new StudentHistory();
        AddDays(history, 1, 4);

        foreach (Subject subject in Subjects.All)
        {
            bool attended = subject != Subject.Programming;
            history.SaveLesson(5, subject, attended, asked: !attended);
        }

        AddDays(history, 6, ObservationDays);

        bool[] decision = _strategy.DecideDay(FirstOddDayAfterObservation, history);

        Assert.False(decision[(int)Subject.Programming]);
    }

    [Fact]
    public void SameHistory_GivesSameDecision()
    {
        var history = new StudentHistory();
        AddDays(history, 1, ObservationDays, Subject.LinearAlgebra, Subject.English);
        var anotherStrategy = new EvenDaysStrategy(ObservationDays);

        bool[] first = _strategy.DecideDay(FirstOddDayAfterObservation, history);
        bool[] second = _strategy.DecideDay(FirstOddDayAfterObservation, history);
        bool[] fromAnotherStrategy = anotherStrategy.DecideDay(FirstOddDayAfterObservation, history);

        Assert.Equal(first, second);
        Assert.Equal(first, fromAnotherStrategy);
    }

    [Fact]
    public void DecideDay_ReturnsDecisionForEverySubject()
    {
        Assert.Equal(Subjects.Count, _strategy.DecideDay(1, new StudentHistory()).Length);
        Assert.Equal(Subjects.Count, _strategy.DecideDay(FirstOddDayAfterObservation, new StudentHistory()).Length);
    }
}
