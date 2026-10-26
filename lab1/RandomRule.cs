namespace Lab1;

public class RandomRule(Random random) : ITeacherRule
{
    private readonly Random _random = random;

    public string Description => "спрашивает случайно с вероятностью 50%";

    public bool ShouldAsk(bool[] askedYesterday)
    {
        return _random.Next(2) == 0;
    }
}
