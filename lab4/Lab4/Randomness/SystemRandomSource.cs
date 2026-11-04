namespace Lab4.Randomness;

// Зерно и число выданных значений — чтобы после перезапуска продолжить ту же последовательность.
public class SystemRandomSource : IRandomSource
{
    private readonly Random _random;

    public SystemRandomSource()
        : this(Random.Shared.Next(), 0)
    {
    }

    public SystemRandomSource(int seed, int draws)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(draws);

        _random = new Random(seed);
        Seed = seed;

        // Любой Next берёт из последовательности ровно одно число.
        for (int i = 0; i < draws; i++)
        {
            _random.Next();
        }

        Draws = draws;
    }

    public int Seed { get; }

    public int Draws { get; private set; }

    public int Next(int maxValue)
    {
        Draws++;
        return _random.Next(maxValue);
    }
}
