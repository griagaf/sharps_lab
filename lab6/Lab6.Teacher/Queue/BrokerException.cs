namespace Lab6.Teacher.Queue;

/// <summary>Пару не удалось передать в очередь Истории. Это проблема не преподавателя, а брокера.</summary>
public class BrokerException : Exception
{
    public BrokerException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
