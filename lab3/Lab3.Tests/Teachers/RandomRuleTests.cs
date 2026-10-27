using Lab3.Teachers;
using Lab3.Tests.Fakes;

namespace Lab3.Tests.Teachers;

public class RandomRuleTests
{
    [Theory]
    [InlineData(0, true)]
    [InlineData(1, false)]
    public void ShouldAsk_DependsOnRandomNumber(int randomNumber, bool expected)
    {
        var rule = new RandomRule(new FakeRandomSource(randomNumber));

        Assert.Equal(expected, rule.ShouldAsk(Flags.None()));
    }

    [Fact]
    public void ShouldAsk_TakesNumberFromZeroToOne()
    {
        var random = new FakeRandomSource(0);
        var rule = new RandomRule(random);

        rule.ShouldAsk(Flags.None());

        Assert.Equal(new[] { 2 }, random.RequestedMaxValues);
    }

    [Fact]
    public void ShouldAsk_IgnoresYesterday()
    {
        var rule = new RandomRule(new FakeRandomSource(1, 1));

        Assert.False(rule.ShouldAsk(Flags.All()));
        Assert.False(rule.ShouldAsk(Flags.None()));
    }

    [Fact]
    public void ShouldAsk_FollowsSequenceOfRandomNumbers()
    {
        var rule = new RandomRule(new FakeRandomSource(0, 1, 1, 0, 0));
        var answers = new List<bool>();

        for (int day = 1; day <= 5; day++)
        {
            answers.Add(rule.ShouldAsk(Flags.None()));
        }

        Assert.Equal(new[] { true, false, false, true, true }, answers);
    }
}
