namespace Lab3.Randomness;

public class SystemRandomSource : IRandomSource
{
    private readonly Random _random = new Random();

    public int Next(int maxValue)
    {
        return _random.Next(maxValue);
    }
}
