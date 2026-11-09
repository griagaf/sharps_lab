using Lab6.Contracts;

namespace Lab6.History;

public interface IHistoryStore
{
    /// <summary>Запоминает, спросили ли на паре. Повторная запись той же пары заменяет прежнюю.</summary>
    void Save(LessonKey lesson, bool asked);

    /// <summary>Спросили ли на паре. Если записи ещё нет, ждёт её появления.</summary>
    /// <returns>Спросили ли на паре или null, если запись не появилась за <paramref name="timeout"/>.</returns>
    Task<bool?> WaitAsync(LessonKey lesson, TimeSpan timeout, CancellationToken cancellationToken);
}
