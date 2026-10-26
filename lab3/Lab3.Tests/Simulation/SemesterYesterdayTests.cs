using Lab3.Domain;
using Lab3.Simulation;
using Lab3.Students;
using Lab3.Teachers;
using Lab3.Tests.Fakes;

namespace Lab3.Tests.Simulation;

public class SemesterYesterdayTests
{
    private static ScriptedRule[] NeverAskingRules()
    {
        var rules = new ScriptedRule[Subjects.Count];

        for (int i = 0; i < rules.Length; i++)
        {
            rules[i] = ScriptedRule.Always(false);
        }

        return rules;
    }

    [Fact]
    public void FirstDay_NobodyWasAskedYesterday()
    {
        ScriptedRule[] rules = NeverAskingRules();
        var semester = new Semester(rules, new FakeStrategy(Flags.All()));

        semester.PlayNextDay();

        foreach (ScriptedRule rule in rules)
        {
            Assert.Equal(Flags.None(), rule.ReceivedYesterdays[0]);
        }
    }

    [Fact]
    public void SecondDay_TeachersSeeAnswersOfFirstDay()
    {
        ScriptedRule[] rules = NeverAskingRules();
        rules[(int)Subject.Calculus] = new ScriptedRule(true, false);
        rules[(int)Subject.Physics] = new ScriptedRule(true, false);
        var semester = new Semester(rules, new FakeStrategy(Flags.All()));

        semester.PlayNextDay();
        semester.PlayNextDay();

        foreach (ScriptedRule rule in rules)
        {
            Assert.Equal(Flags.Only(Subject.Calculus, Subject.Physics), rule.ReceivedYesterdays[1]);
        }
    }

    [Fact]
    public void EveryDay_YesterdayIsOnlyThePreviousDay()
    {
        ScriptedRule[] rules = NeverAskingRules();
        rules[(int)Subject.English] = new ScriptedRule(true, false, true, true);
        var semester = new Semester(rules, new FakeStrategy(Flags.All()));

        for (int i = 0; i < 5; i++)
        {
            semester.PlayNextDay();
        }

        ScriptedRule calculus = rules[(int)Subject.Calculus];
        Assert.Equal(Flags.None(), calculus.ReceivedYesterdays[0]);
        Assert.Equal(Flags.Only(Subject.English), calculus.ReceivedYesterdays[1]);
        Assert.Equal(Flags.None(), calculus.ReceivedYesterdays[2]);
        Assert.Equal(Flags.Only(Subject.English), calculus.ReceivedYesterdays[3]);
        Assert.Equal(Flags.Only(Subject.English), calculus.ReceivedYesterdays[4]);
    }

    [Fact]
    public void SkippedLessons_DoNotChangeYesterday()
    {
        ScriptedRule[] rules = NeverAskingRules();
        rules[(int)Subject.Calculus] = new ScriptedRule(true, false);
        var strategy = new FakeStrategy(Flags.AllExcept(Subject.Physics, Subject.English));
        var semester = new Semester(rules, strategy);

        semester.PlayNextDay();
        semester.PlayNextDay();

        foreach (ScriptedRule rule in rules)
        {
            Assert.Equal(Flags.Only(Subject.Calculus), rule.ReceivedYesterdays[1]);
        }
    }

    // Линал повторяет вчерашний матан, дискра — вчерашний линал.
    // Если бы "вчера" обновлялось прямо во время опроса, вопрос пробежал бы по цепочке за один день.
    [Fact]
    public void AllTeachersSeeTheSameYesterday()
    {
        var rules = new ITeacherRule[Subjects.Count];
        rules[(int)Subject.Calculus] = new ScriptedRule(true, false);
        rules[(int)Subject.LinearAlgebra] = new CopyYesterdayRule(Subject.Calculus);
        rules[(int)Subject.DiscreteMath] = new CopyYesterdayRule(Subject.LinearAlgebra);
        rules[(int)Subject.Programming] = ScriptedRule.Always(false);
        rules[(int)Subject.Physics] = ScriptedRule.Always(false);
        rules[(int)Subject.English] = ScriptedRule.Always(false);
        var strategy = new FakeStrategy(Flags.All());
        var semester = new Semester(rules, strategy);

        semester.PlayNextDay();
        semester.PlayNextDay();
        semester.PlayNextDay();

        Assert.NotNull(strategy.History);
        Assert.Equal(new bool?[] { true, false, false }, AskedOnDays(strategy.History, Subject.Calculus, 3));
        Assert.Equal(new bool?[] { false, true, false }, AskedOnDays(strategy.History, Subject.LinearAlgebra, 3));
        Assert.Equal(new bool?[] { false, false, true }, AskedOnDays(strategy.History, Subject.DiscreteMath, 3));
    }

    private static bool?[] AskedOnDays(IReadOnlyStudentHistory history, Subject subject, int days)
    {
        var result = new bool?[days];

        for (int day = 1; day <= days; day++)
        {
            result[day - 1] = history.WasAsked(day, subject);
        }

        return result;
    }
}
