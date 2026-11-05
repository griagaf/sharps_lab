namespace Lab5.Teacher.History;

/// <summary>
/// История не дала преподавателю того, что нужно для пары: недоступна, ответила ошибкой
/// или непонятно, либо в ней нет записи за вчера. Это проблема не преподавателя, а того, на кого он опирается.
/// </summary>
public class HistoryException : Exception
{
    public HistoryException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
