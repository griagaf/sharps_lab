using Lab3.Teachers;

namespace Lab3.Tests.Fakes;

// Отвечает по заданному списку: первый вызов — первый ответ и т.д.
// Если ответы кончились, повторяет последний.
public class ScriptedRule : ITeacherRule
{
    private readonly bool[] _answers;

    public ScriptedRule(params bool[] answers)
    {
        if (answers.Length == 0)
        {
            throw new ArgumentException("Нужен хотя бы один ответ");
        }

        _answers = answers;
    }

    public List<bool[]> ReceivedYesterdays { get; } = new List<bool[]>();

    public int CallsCount => ReceivedYesterdays.Count;

    public string Description => "правило для тестов";

    public bool ShouldAsk(bool[] askedYesterday)
    {
        int index = Math.Min(CallsCount, _answers.Length - 1);

        // копия, потому что семестр может потом поменять массив
        ReceivedYesterdays.Add((bool[])askedYesterday.Clone());

        return _answers[index];
    }

    public static ScriptedRule Always(bool asks)
    {
        return new ScriptedRule(asks);
    }
}
