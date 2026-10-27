using Lab3.Students;

namespace Lab3.Tests.Fakes;

// Решения на каждый день задаются заранее. Если дни кончились, повторяется последнее решение.
public class FakeStrategy : ISkipStrategy
{
    private readonly bool[][] _decisions;

    public FakeStrategy(params bool[][] decisionsByDay)
    {
        if (decisionsByDay.Length == 0)
        {
            throw new ArgumentException("Нужно хотя бы одно решение");
        }

        _decisions = decisionsByDay;
    }

    public List<int> CalledDays { get; } = new List<int>();

    public IReadOnlyStudentHistory? History { get; private set; }

    public string Name => "стратегия для тестов";

    public bool[] DecideDay(int day, IReadOnlyStudentHistory history)
    {
        CalledDays.Add(day);
        History = history;

        int index = Math.Min(day - 1, _decisions.Length - 1);
        return (bool[])_decisions[index].Clone();
    }
}
