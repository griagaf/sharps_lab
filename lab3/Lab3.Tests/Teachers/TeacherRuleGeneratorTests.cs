using Lab3.Domain;
using Lab3.Teachers;
using Lab3.Tests.Fakes;

namespace Lab3.Tests.Teachers;

public class TeacherRuleGeneratorTests
{
    [Fact]
    public void CreateForAllSubjects_ReturnsRuleForEachSubject()
    {
        var generator = new TeacherRuleGenerator(new FakeRandomSource(0, 0, 0, 0, 0, 0));

        ITeacherRule[] rules = generator.CreateForAllSubjects();

        Assert.Equal(Subjects.Count, rules.Length);
    }

    [Fact]
    public void CreateForAllSubjects_PicksRuleTypeByRandomNumber()
    {
        // матан: правило 1; линал: правило 2 (A = физика); дискра: правило 3 (A = матан, Б = английский); остальные: правило 1
        var random = new FakeRandomSource(0, 1, 4, 2, 0, 5, 0, 0, 0);
        var generator = new TeacherRuleGenerator(random);

        ITeacherRule[] rules = generator.CreateForAllSubjects();

        Assert.IsType<RandomRule>(rules[(int)Subject.Calculus]);
        Assert.IsType<CopyYesterdayRule>(rules[(int)Subject.LinearAlgebra]);
        Assert.IsType<XorYesterdayRule>(rules[(int)Subject.DiscreteMath]);
        Assert.IsType<RandomRule>(rules[(int)Subject.Programming]);
        Assert.IsType<RandomRule>(rules[(int)Subject.Physics]);
        Assert.IsType<RandomRule>(rules[(int)Subject.English]);
    }

    [Fact]
    public void CreateForAllSubjects_CopyRuleWatchesRandomSubject()
    {
        var generator = new TeacherRuleGenerator(new FakeRandomSource(1, 4, 0, 0, 0, 0, 0));

        ITeacherRule rule = generator.CreateForAllSubjects()[0];

        Assert.True(rule.ShouldAsk(Flags.Only(Subject.Physics)));
        Assert.False(rule.ShouldAsk(Flags.AllExcept(Subject.Physics)));
    }

    [Fact]
    public void CreateForAllSubjects_XorRulePicksAnotherSubjectWhenSame()
    {
        // A = линал, первый Б тоже линал, поэтому берётся следующее число: Б = программирование
        var random = new FakeRandomSource(2, 1, 1, 3, 0, 0, 0, 0, 0);
        var generator = new TeacherRuleGenerator(random);

        ITeacherRule rule = generator.CreateForAllSubjects()[0];

        Assert.True(rule.ShouldAsk(Flags.Only(Subject.LinearAlgebra)));
        Assert.True(rule.ShouldAsk(Flags.Only(Subject.Programming)));
        Assert.False(rule.ShouldAsk(Flags.Only(Subject.LinearAlgebra, Subject.Programming)));
        Assert.False(rule.ShouldAsk(Flags.None()));
        Assert.Equal(new[] { 3, 6, 6, 6 }, random.RequestedMaxValues.Take(4));
    }

    [Fact]
    public void CreateForAllSubjects_RandomRuleUsesSameRandomSource()
    {
        var random = new FakeRandomSource(0, 0, 0, 0, 0, 0, 1, 0);
        var generator = new TeacherRuleGenerator(random);

        ITeacherRule rule = generator.CreateForAllSubjects()[0];

        Assert.False(rule.ShouldAsk(Flags.None()));
        Assert.True(rule.ShouldAsk(Flags.None()));
    }

    [Fact]
    public void CreateForAllSubjects_SameNumbersGiveSameRules()
    {
        int[] numbers = { 1, 2, 2, 0, 5, 0, 1, 3, 2, 4, 1, 0 };
        ITeacherRule[] first = new TeacherRuleGenerator(new FakeRandomSource(numbers)).CreateForAllSubjects();
        ITeacherRule[] second = new TeacherRuleGenerator(new FakeRandomSource(numbers)).CreateForAllSubjects();

        for (int i = 0; i < Subjects.Count; i++)
        {
            Assert.Equal(first[i].GetType(), second[i].GetType());
            Assert.Equal(first[i].Description, second[i].Description);
        }
    }
}
