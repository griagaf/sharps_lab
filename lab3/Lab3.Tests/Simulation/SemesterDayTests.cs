using Lab3.Domain;
using Lab3.Simulation;
using Lab3.Teachers;
using Lab3.Tests.Fakes;

namespace Lab3.Tests.Simulation;

public class SemesterDayTests
{
    private static ITeacherRule[] RulesAskingOnly(params Subject[] askedSubjects)
    {
        var rules = new ITeacherRule[Subjects.Count];

        foreach (Subject subject in Subjects.All)
        {
            rules[(int)subject] = ScriptedRule.Always(askedSubjects.Contains(subject));
        }

        return rules;
    }

    // Остальные пять предметов: студент на паре, там не спрашивают.
    [Theory]
    [InlineData(true, false, false, 1)]
    [InlineData(true, true, false, 1)]
    [InlineData(false, false, false, 2)]
    [InlineData(false, true, true, 0)]
    public void AttendedAndAskedCombinations(bool attended, bool asked, bool expectedExpelled, int expectedPleasure)
    {
        ITeacherRule[] rules = asked ? RulesAskingOnly(Subject.Physics) : RulesAskingOnly();
        bool[] decision = attended ? Flags.All() : Flags.AllExcept(Subject.Physics);
        var semester = new Semester(rules, new FakeStrategy(decision));

        semester.PlayNextDay();

        Assert.Equal(expectedExpelled, semester.IsExpelled);
        Assert.Equal(expectedPleasure, semester.TotalPleasure);
        Assert.Equal(1, semester.DaysPassed);
    }

    [Fact]
    public void Attended_NotAsked_OnlyPie()
    {
        var semester = new Semester(RulesAskingOnly(), new FakeStrategy(Flags.All()));

        List<Subject> skipped = semester.PlayNextDay();

        Assert.Empty(skipped);
        Assert.Equal(Semester.PleasureForPie, semester.TotalPleasure);
        Assert.False(semester.IsExpelled);
    }

    [Fact]
    public void Attended_Asked_NothingBadHappens()
    {
        var semester = new Semester(RulesAskingOnly(Subjects.All), new FakeStrategy(Flags.All()));

        semester.PlayNextDay();

        Assert.False(semester.IsExpelled);
        Assert.Equal(Semester.PleasureForPie, semester.TotalPleasure);
    }

    [Fact]
    public void Skipped_NotAsked_PleasureForSkipAndPie()
    {
        var semester = new Semester(RulesAskingOnly(), new FakeStrategy(Flags.AllExcept(Subject.English)));

        List<Subject> skipped = semester.PlayNextDay();

        Assert.Equal(new[] { Subject.English }, skipped);
        Assert.Equal(Semester.PleasureForSkip + Semester.PleasureForPie, semester.TotalPleasure);
        Assert.False(semester.IsExpelled);
    }

    [Fact]
    public void Skipped_Asked_Expelled()
    {
        var semester = new Semester(RulesAskingOnly(Subject.English), new FakeStrategy(Flags.AllExcept(Subject.English)));

        semester.PlayNextDay();

        Assert.True(semester.IsExpelled);
        Assert.Equal(Subject.English, semester.ExpelledSubject);
        Assert.Equal(0, semester.TotalPleasure);
        Assert.Equal(1, semester.DaysPassed);
    }

    [Fact]
    public void Pleasure_AddsUpOverDays()
    {
        var strategy = new FakeStrategy(Flags.AllExcept(Subject.Physics, Subject.English));
        var semester = new Semester(RulesAskingOnly(Subject.Calculus), strategy);

        for (int i = 0; i < 3; i++)
        {
            semester.PlayNextDay();
        }

        Assert.Equal(9, semester.TotalPleasure);
        Assert.Equal(3, semester.DaysPassed);
        Assert.Equal(3.0, semester.AveragePleasurePerDay);
    }

    [Fact]
    public void AverageBeforeFirstDay_IsZero()
    {
        var semester = new Semester(RulesAskingOnly(), new FakeStrategy(Flags.All()));

        Assert.Equal(0.0, semester.AveragePleasurePerDay);
    }

    [Fact]
    public void StrategyIsCalledOncePerDay_WithDayNumber()
    {
        var strategy = new FakeStrategy(Flags.All());
        var semester = new Semester(RulesAskingOnly(), strategy);

        semester.PlayNextDay();
        semester.PlayNextDay();
        semester.PlayNextDay();

        Assert.Equal(new[] { 1, 2, 3 }, strategy.CalledDays);
    }

    [Fact]
    public void History_SkippedLessonHasNoAnswer()
    {
        var strategy = new FakeStrategy(Flags.AllExcept(Subject.English));
        var semester = new Semester(RulesAskingOnly(Subject.Calculus), strategy);

        semester.PlayNextDay();
        semester.PlayNextDay();

        Assert.NotNull(strategy.History);
        Assert.True(strategy.History.Attended(1, Subject.Calculus));
        Assert.True(strategy.History.WasAsked(1, Subject.Calculus));
        Assert.False(strategy.History.WasAsked(1, Subject.Physics));
        Assert.False(strategy.History.Attended(1, Subject.English));
        Assert.Null(strategy.History.WasAsked(1, Subject.English));
    }

    [Fact]
    public void Constructor_WithWrongNumberOfRules_Throws()
    {
        var rules = new ITeacherRule[] { ScriptedRule.Always(false), ScriptedRule.Always(false) };

        Assert.Throws<ArgumentException>(() => new Semester(rules, new FakeStrategy(Flags.All())));
    }

    [Fact]
    public void StrategyWithWrongNumberOfDecisions_Throws()
    {
        var semester = new Semester(RulesAskingOnly(), new FakeStrategy(new bool[] { true, true }));

        Assert.Throws<InvalidOperationException>(() => semester.PlayNextDay());
    }
}
