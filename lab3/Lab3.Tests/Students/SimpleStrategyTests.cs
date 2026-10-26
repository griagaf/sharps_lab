using Lab3.Domain;
using Lab3.Students;

namespace Lab3.Tests.Students;

public class SimpleStrategyTests
{
    private const int ObservationDays = 15;
    private const int FirstDayAfterObservation = ObservationDays + 1;

    private readonly SimpleStrategy _strategy = new SimpleStrategy(ObservationDays);

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

    [Fact]
    public void EmptyHistory_FirstDay_AttendsEverything()
    {
        bool[] decision = _strategy.DecideDay(1, new StudentHistory());

        Assert.Equal(Flags.All(), decision);
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

    [Fact]
    public void EmptyHistory_AfterObservation_SkipsEverything()
    {
        bool[] decision = _strategy.DecideDay(FirstDayAfterObservation, new StudentHistory());

        Assert.Equal(Flags.None(), decision);
    }

    [Fact]
    public void NeverAsked_SkipsEverything()
    {
        var history = new StudentHistory();
        AddDays(history, 1, ObservationDays);

        Assert.Equal(Flags.None(), _strategy.DecideDay(FirstDayAfterObservation, history));
    }

    [Fact]
    public void AlwaysAsked_AttendsEverything()
    {
        var history = new StudentHistory();
        AddDays(history, 1, ObservationDays, Subjects.All);

        Assert.Equal(Flags.All(), _strategy.DecideDay(FirstDayAfterObservation, history));
    }

    [Fact]
    public void AlwaysAsked_StillAttendsLaterInSemester()
    {
        var history = new StudentHistory();
        AddDays(history, 1, 99, Subjects.All);

        Assert.Equal(Flags.All(), _strategy.DecideDay(100, history));
    }

    [Fact]
    public void MixedHistory_AttendsOnlySubjectsThatWereAsked()
    {
        var history = new StudentHistory();
        AddDays(history, 1, 2);
        AddDays(history, 3, 3, Subject.Calculus);
        AddDays(history, 4, 10);
        AddDays(history, 11, 11, Subject.English, Subject.Calculus);
        AddDays(history, 12, 14);
        AddDays(history, 15, 15, Subject.Physics);

        bool[] decision = _strategy.DecideDay(FirstDayAfterObservation, history);

        Assert.Equal(Flags.Only(Subject.Calculus, Subject.Physics, Subject.English), decision);
    }

    [Fact]
    public void AskedOnlyOnce_IsEnoughToKeepAttending()
    {
        var history = new StudentHistory();
        AddDays(history, 1, 1, Subject.DiscreteMath);
        AddDays(history, 2, ObservationDays);

        bool[] decision = _strategy.DecideDay(FirstDayAfterObservation, history);

        Assert.Equal(Flags.Only(Subject.DiscreteMath), decision);
    }

    [Fact]
    public void SkippedLesson_IsNotCountedAsAsked()
    {
        var history = new StudentHistory();
        AddDays(history, 1, 4);

        foreach (Subject subject in Subjects.All)
        {
            bool attended = subject != Subject.Programming;
            history.SaveLesson(5, subject, attended, asked: !attended);
        }

        AddDays(history, 6, ObservationDays);

        bool[] decision = _strategy.DecideDay(FirstDayAfterObservation, history);

        Assert.False(decision[(int)Subject.Programming]);
    }

    [Fact]
    public void SameHistory_GivesSameDecision()
    {
        var history = new StudentHistory();
        AddDays(history, 1, ObservationDays, Subject.LinearAlgebra, Subject.English);
        var anotherStrategy = new SimpleStrategy(ObservationDays);

        bool[] first = _strategy.DecideDay(FirstDayAfterObservation, history);
        bool[] second = _strategy.DecideDay(FirstDayAfterObservation, history);
        bool[] fromAnotherStrategy = anotherStrategy.DecideDay(FirstDayAfterObservation, history);

        Assert.Equal(first, second);
        Assert.Equal(first, fromAnotherStrategy);
    }

    [Theory]
    [InlineData(Subject.Calculus)]
    [InlineData(Subject.LinearAlgebra)]
    [InlineData(Subject.DiscreteMath)]
    [InlineData(Subject.Programming)]
    [InlineData(Subject.Physics)]
    [InlineData(Subject.English)]
    public void EachSubject_IsDecidedByItsOwnHistory(Subject askedSubject)
    {
        var history = new StudentHistory();
        AddDays(history, 1, ObservationDays, askedSubject);

        bool[] decision = _strategy.DecideDay(FirstDayAfterObservation, history);

        Assert.Equal(Flags.Only(askedSubject), decision);
    }

    [Fact]
    public void ChangingOneSubject_DoesNotChangeOthers()
    {
        var withoutPhysics = new StudentHistory();
        AddDays(withoutPhysics, 1, ObservationDays, Subject.Calculus);
        var withPhysics = new StudentHistory();
        AddDays(withPhysics, 1, ObservationDays, Subject.Calculus, Subject.Physics);

        bool[] first = _strategy.DecideDay(FirstDayAfterObservation, withoutPhysics);
        bool[] second = _strategy.DecideDay(FirstDayAfterObservation, withPhysics);

        foreach (Subject subject in Subjects.All)
        {
            if (subject != Subject.Physics)
            {
                Assert.Equal(first[(int)subject], second[(int)subject]);
            }
        }

        Assert.False(first[(int)Subject.Physics]);
        Assert.True(second[(int)Subject.Physics]);
    }

    [Fact]
    public void DecideDay_ReturnsDecisionForEverySubject()
    {
        Assert.Equal(Subjects.Count, _strategy.DecideDay(1, new StudentHistory()).Length);
        Assert.Equal(Subjects.Count, _strategy.DecideDay(FirstDayAfterObservation, new StudentHistory()).Length);
    }
}
