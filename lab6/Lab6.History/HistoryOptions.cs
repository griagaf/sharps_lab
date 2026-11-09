using System.ComponentModel.DataAnnotations;

namespace Lab6.History;

/// <summary>Настройки Истории, секция «History».</summary>
public class HistoryOptions
{
    public const string SectionName = "History";

    /// <summary>Сколько GET ждёт пару, которой ещё нет: запись приходит из очереди с задержкой.</summary>
    [Range(typeof(TimeSpan), "00:00:00", "00:01:00", ErrorMessage = "ReadTimeout должен быть от 0 до 1 минуты")]
    public TimeSpan ReadTimeout { get; set; } = TimeSpan.FromSeconds(5);
}
