using Lab3.Randomness;

namespace Lab3.Tests.Fakes;

// Отдаёт заранее заданные числа по очереди, чтобы тесты всегда шли одинаково.
public class FakeRandomSource : IRandomSource
{
    private readonly Queue<int> _values;

    public FakeRandomSource(params int[] values)
    {
        _values = new Queue<int>(values);
    }

    public List<int> RequestedMaxValues { get; } = new List<int>();

    public int Next(int maxValue)
    {
        RequestedMaxValues.Add(maxValue);

        if (_values.Count == 0)
        {
            throw new InvalidOperationException("Закончились заготовленные случайные числа");
        }

        int value = _values.Dequeue();

        if (value < 0 || value >= maxValue)
        {
            throw new InvalidOperationException($"Число {value} не подходит для Next({maxValue})");
        }

        return value;
    }
}
