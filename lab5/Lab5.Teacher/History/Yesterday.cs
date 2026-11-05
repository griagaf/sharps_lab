using Lab5.Contracts;
using Lab5.Teacher.Rules;

namespace Lab5.Teacher.History;

/// <summary>Вчерашний день конкретного семестра, прочитанный из Истории.</summary>
public class Yesterday : IYesterday
{
    private readonly IHistoryClient _history;
    private readonly Guid _semesterId;
    private readonly int _today;

    public Yesterday(IHistoryClient history, Guid semesterId, int today)
    {
        _history = history;
        _semesterId = semesterId;
        _today = today;
    }

    public async Task<bool> WasAskedAsync(Subject subject, CancellationToken cancellationToken)
    {
        if (_today == 1)
        {
            return false;
        }

        var lesson = new LessonKey(_semesterId, _today - 1, subject);

        // Студент начинает день только после того, как прошли все пары предыдущего,
        // поэтому отсутствие записи означает, что вчерашний день так и не был проведён.
        return await _history.FindAskedAsync(lesson, cancellationToken)
            ?? throw new InvalidOperationException($"В Истории нет записи о паре {lesson}");
    }
}
