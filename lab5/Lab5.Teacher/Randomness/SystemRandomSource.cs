namespace Lab5.Teacher.Randomness;

public class SystemRandomSource : IRandomSource
{
    public int Next(int maxValue)
    {
        return Random.Shared.Next(maxValue);
    }
}
