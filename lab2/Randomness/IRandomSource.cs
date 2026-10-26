namespace Lab2.Randomness;

public interface IRandomSource
{
    /// <summary>Случайное число от 0 до maxValue - 1.</summary>
    int Next(int maxValue);
}
