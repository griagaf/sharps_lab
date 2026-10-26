using Lab3.Domain;
using Lab3.Teachers;

namespace Lab3.Tests.Teachers;

public class XorYesterdayRuleTests
{
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, false)]
    public void ShouldAsk_WhenExactlyOneOfTwoSubjectsWasAsked(bool askedA, bool askedB, bool expected)
    {
        var rule = new XorYesterdayRule(Subject.Calculus, Subject.English);
        bool[] yesterday = Flags.None();
        yesterday[(int)Subject.Calculus] = askedA;
        yesterday[(int)Subject.English] = askedB;

        Assert.Equal(expected, rule.ShouldAsk(yesterday));
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, false)]
    public void ShouldAsk_IgnoresOtherSubjects(bool askedA, bool askedB, bool expected)
    {
        var rule = new XorYesterdayRule(Subject.LinearAlgebra, Subject.Physics);
        bool[] yesterday = Flags.All();
        yesterday[(int)Subject.LinearAlgebra] = askedA;
        yesterday[(int)Subject.Physics] = askedB;

        Assert.Equal(expected, rule.ShouldAsk(yesterday));
    }

    [Fact]
    public void ShouldAsk_OnSequenceOfDays()
    {
        var rule = new XorYesterdayRule(Subject.Programming, Subject.DiscreteMath);
        bool[][] yesterdays =
        {
            Flags.None(),
            Flags.Only(Subject.Programming),
            Flags.Only(Subject.Programming, Subject.DiscreteMath),
            Flags.Only(Subject.DiscreteMath),
            Flags.Only(Subject.English),
        };
        var answers = new List<bool>();

        foreach (bool[] yesterday in yesterdays)
        {
            answers.Add(rule.ShouldAsk(yesterday));
        }

        Assert.Equal(new[] { false, true, false, true, false }, answers);
    }

    [Fact]
    public void Constructor_WithSameSubjects_Throws()
    {
        Assert.Throws<ArgumentException>(() => new XorYesterdayRule(Subject.Physics, Subject.Physics));
    }
}
