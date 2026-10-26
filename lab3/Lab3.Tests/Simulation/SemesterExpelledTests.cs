using Lab3.Domain;
using Lab3.Simulation;
using Lab3.Teachers;
using Lab3.Tests.Fakes;

namespace Lab3.Tests.Simulation;

public class SemesterExpelledTests
{
    private readonly ScriptedRule[] _rules = new ScriptedRule[Subjects.Count];
    private readonly FakeStrategy _strategy;
    private readonly Semester _semester;

    // Студент каждый день прогуливает физику и английский.
    // По английскому никогда не спрашивают, по физике спросят на третий день.
    public SemesterExpelledTests()
    {
        for (int i = 0; i < _rules.Length; i++)
        {
            _rules[i] = ScriptedRule.Always(false);
        }

        _rules[(int)Subject.Physics] = new ScriptedRule(false, false, true);
        _strategy = new FakeStrategy(Flags.AllExcept(Subject.Physics, Subject.English));
        _semester = new Semester(_rules, _strategy);
    }

    private void PlayDays(int count)
    {
        for (int i = 0; i < count; i++)
        {
            _semester.PlayNextDay();
        }
    }

    [Fact]
    public void BeforeExpulsion_PleasureIsCollected()
    {
        PlayDays(2);

        Assert.False(_semester.IsExpelled);
        Assert.Equal(6, _semester.TotalPleasure);
    }

    [Fact]
    public void Expulsion_ResetsPleasureToZero()
    {
        PlayDays(3);

        Assert.True(_semester.IsExpelled);
        Assert.Equal(0, _semester.TotalPleasure);
        Assert.Equal(0.0, _semester.AveragePleasurePerDay);
    }

    [Fact]
    public void Expulsion_RemembersDayAndSubject()
    {
        PlayDays(3);

        Assert.Equal(3, _semester.DaysPassed);
        Assert.Equal(Subject.Physics, _semester.ExpelledSubject);
    }

    [Fact]
    public void AfterExpulsion_NextDayIsNotPlayed()
    {
        PlayDays(3);

        Assert.Throws<InvalidOperationException>(() => _semester.PlayNextDay());
        Assert.Throws<InvalidOperationException>(() => _semester.PlayNextDay());
    }

    [Fact]
    public void AfterExpulsion_StateDoesNotChange()
    {
        PlayDays(3);

        Assert.Throws<InvalidOperationException>(() => _semester.PlayNextDay());

        Assert.True(_semester.IsExpelled);
        Assert.Equal(0, _semester.TotalPleasure);
        Assert.Equal(3, _semester.DaysPassed);
    }

    [Fact]
    public void AfterExpulsion_StrategyAndTeachersAreNotCalled()
    {
        PlayDays(3);

        Assert.Throws<InvalidOperationException>(() => _semester.PlayNextDay());

        Assert.Equal(new[] { 1, 2, 3 }, _strategy.CalledDays);

        foreach (ScriptedRule rule in _rules)
        {
            Assert.Equal(3, rule.CallsCount);
        }
    }

    [Fact]
    public void ExpulsionOnFirstSubject_RestOfDayIsNotCounted()
    {
        var rules = new ITeacherRule[Subjects.Count];

        for (int i = 0; i < rules.Length; i++)
        {
            rules[i] = ScriptedRule.Always(false);
        }

        rules[(int)Subject.Calculus] = ScriptedRule.Always(true);
        var semester = new Semester(rules, new FakeStrategy(Flags.None()));

        List<Subject> skipped = semester.PlayNextDay();

        Assert.True(semester.IsExpelled);
        Assert.Equal(Subject.Calculus, semester.ExpelledSubject);
        Assert.Equal(new[] { Subject.Calculus }, skipped);
        Assert.Equal(0, semester.TotalPleasure);
    }

    [Fact]
    public void ExpulsionAfterSuccessfulSkipsSameDay_PleasureIsStillZero()
    {
        var rules = new ITeacherRule[Subjects.Count];

        for (int i = 0; i < rules.Length; i++)
        {
            rules[i] = ScriptedRule.Always(false);
        }

        rules[(int)Subject.English] = ScriptedRule.Always(true);
        var semester = new Semester(rules, new FakeStrategy(Flags.None()));

        semester.PlayNextDay();

        Assert.True(semester.IsExpelled);
        Assert.Equal(Subject.English, semester.ExpelledSubject);
        Assert.Equal(0, semester.TotalPleasure);
    }
}
