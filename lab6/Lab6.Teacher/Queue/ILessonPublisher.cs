using Lab6.Contracts;

namespace Lab6.Teacher.Queue;

public interface ILessonPublisher
{
    /// <summary>Кладёт пару в очередь Истории и ждёт, пока брокер подтвердит, что сохранил её.</summary>
    /// <exception cref="BrokerException">Брокер недоступен, отказал или не подтвердил вовремя.</exception>
    Task PublishAsync(LessonHeld lesson, CancellationToken cancellationToken);
}
