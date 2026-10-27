using Lab3.Domain;
using Lab3.Teachers;

namespace Lab3.Tests.Teachers;

public class CopyYesterdayRuleTests
{
    [Theory]
    [InlineData(Subject.Calculus)]
    [InlineData(Subject.LinearAlgebra)]
    [InlineData(Subject.DiscreteMath)]
    [InlineData(Subject.Programming)]
    [InlineData(Subject.Physics)]
    [InlineData(Subject.English)]
    public void ShouldAsk_WhenOnlySubjectAWasAsked_ReturnsTrue(Subject subjectA)
    {
        var rule = new CopyYesterdayRule(subjectA);

        Assert.True(rule.ShouldAsk(Flags.Only(subjectA)));
    }

    [Theory]
    [InlineData(Subject.Calculus)]
    [InlineData(Subject.LinearAlgebra)]
    [InlineData(Subject.DiscreteMath)]
    [InlineData(Subject.Programming)]
    [InlineData(Subject.Physics)]
    [InlineData(Subject.English)]
    public void ShouldAsk_WhenEveryoneExceptSubjectAWasAsked_ReturnsFalse(Subject subjectA)
    {
        var rule = new CopyYesterdayRule(subjectA);

        Assert.False(rule.ShouldAsk(Flags.AllExcept(subjectA)));
    }

    [Fact]
    public void ShouldAsk_WhenNobodyWasAsked_ReturnsFalse()
    {
        var rule = new CopyYesterdayRule(Subject.Physics);

        Assert.False(rule.ShouldAsk(Flags.None()));
    }

    [Fact]
    public void ShouldAsk_RepeatsSubjectAFromYesterday()
    {
        var rule = new CopyYesterdayRule(Subject.Physics);
        bool[] physicsAskedByDay = { true, false, true, true, false, false, true };
        var answers = new List<bool>();

        foreach (bool physicsAsked in physicsAskedByDay)
        {
            bool[] yesterday = physicsAsked ? Flags.Only(Subject.Physics) : Flags.None();
            answers.Add(rule.ShouldAsk(yesterday));
        }

        Assert.Equal(physicsAskedByDay, answers);
    }
}
