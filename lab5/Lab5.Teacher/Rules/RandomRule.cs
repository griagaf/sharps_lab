using Lab5.Teacher.Randomness;

namespace Lab5.Teacher.Rules;

public class RandomRule : ITeacherRule
{
    private readonly IRandomSource _random;

    public RandomRule(IRandomSource random)
    {
        _random = random;
    }

    public string Description => "спрашивает случайно с вероятностью 50%";

    public Task<bool> ShouldAskAsync(IYesterday yesterday, CancellationToken cancellationToken)
    {
        return Task.FromResult(_random.Next(2) == 0);
    }
}
