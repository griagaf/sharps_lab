using System.ComponentModel.DataAnnotations;

namespace Lab6.History;

/// <summary>Настройки Истории, секция «History».</summary>
public class HistoryOptions : IValidatableObject
{
    public const string SectionName = "History";

    /// <summary>Адрес RabbitMQ вместе с логином и паролем, например amqp://user:pass@host:5672.</summary>
    [Required]
    public Uri? BrokerUrl { get; set; }

    /// <summary>Сколько GET ждёт пару, которой ещё нет: запись приходит из очереди с задержкой.</summary>
    [Range(typeof(TimeSpan), "00:00:00", "00:01:00", ErrorMessage = "ReadTimeout должен быть от 0 до 1 минуты")]
    public TimeSpan ReadTimeout { get; set; } = TimeSpan.FromSeconds(5);

    // Вызывается, только если прошёл [Required], поэтому адрес уже есть.
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!BrokerUrl!.IsAbsoluteUri || BrokerUrl.Scheme is not ("amqp" or "amqps"))
        {
            yield return new ValidationResult($"Адрес RabbitMQ должен быть вида amqp://…: «{BrokerUrl}»", [nameof(BrokerUrl)]);
        }
    }
}
