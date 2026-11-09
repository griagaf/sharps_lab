using Lab6.Contracts;

namespace Lab6.Teacher.History;

public interface IHistoryClient
{
    /// <returns>Спросили ли на паре или null, если в Истории о ней ничего нет.</returns>
    /// <exception cref="HistoryException">Историю не удалось спросить.</exception>
    Task<bool?> FindAskedAsync(LessonKey lesson, CancellationToken cancellationToken);

    /// <exception cref="HistoryException">В Историю не удалось записать.</exception>
    Task SaveAsync(LessonKey lesson, bool asked, CancellationToken cancellationToken);
}
