namespace Lab5.Teacher.History;

public interface IHistoryClient
{
    /// <returns>Спросили ли на паре или null, если в Истории о ней ничего нет.</returns>
    Task<bool?> FindAskedAsync(LessonKey lesson, CancellationToken cancellationToken);

    Task SaveAsync(LessonKey lesson, bool asked, CancellationToken cancellationToken);
}
