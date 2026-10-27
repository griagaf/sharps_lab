using Lab3.Domain;
using Lab3.Students;

namespace Lab3.Tests.Students;

public class EvenDaysStrategyTests
{
    private readonly EvenDaysStrategy _strategy = new EvenDaysStrategy();

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
    [InlineData(3)]
    [InlineData(15)]
    [InlineData(99)]
    public void OddDay_SkipsEverything(int day)
    {
        Assert.Equal(Flags.None(), _strategy.DecideDay(day, new StudentHistory()));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(16)]
    [InlineData(100)]
    public void EvenDay_AttendsEverything(int day)
    {
        Assert.Equal(Flags.All(), _strategy.DecideDay(day, new StudentHistory()));
    }

    [Fact]
    public void AlwaysAsked_StillSkipsOddDay()
    {
        var history = new StudentHistory();
        AddDays(history, 1, 14, Subjects.All);

        Assert.Equal(Flags.None(), _strategy.DecideDay(15, history));
    }

    [Fact]
    public void NeverAsked_StillAttendsEvenDay()
    {
        var history = new StudentHistory();
        AddDays(history, 1, 15);

        Assert.Equal(Flags.All(), _strategy.DecideDay(16, history));
    }

    [Fact]
    public void SameDay_GivesSameDecision()
    {
        var history = new StudentHistory();
        AddDays(history, 1, 5, Subject.LinearAlgebra, Subject.English);
        var anotherStrategy = new EvenDaysStrategy();

        bool[] first = _strategy.DecideDay(6, history);
        bool[] second = _strategy.DecideDay(6, history);
        bool[] fromAnotherStrategy = anotherStrategy.DecideDay(6, new StudentHistory());

        Assert.Equal(first, second);
        Assert.Equal(first, fromAnotherStrategy);
    }

    [Fact]
    public void DecideDay_ReturnsDecisionForEverySubject()
    {
        Assert.Equal(Subjects.Count, _strategy.DecideDay(1, new StudentHistory()).Length);
        Assert.Equal(Subjects.Count, _strategy.DecideDay(2, new StudentHistory()).Length);
    }
}
