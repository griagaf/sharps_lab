namespace Lab2;

public interface IRandomSource
{
    /// <summary>Случайное число от 0 до maxValue - 1.</summary>
    int Next(int maxValue);
}
