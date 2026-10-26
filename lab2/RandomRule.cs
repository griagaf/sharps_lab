namespace Lab2;

public class RandomRule(IRandomSource random) : ITeacherRule
{
    private readonly IRandomSource _random = random;

    public string Description => "спрашивает случайно с вероятностью 50%";

    public bool ShouldAsk(bool[] askedYesterday)
    {
        return _random.Next(2) == 0;
    }
}
