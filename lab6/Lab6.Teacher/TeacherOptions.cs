using System.ComponentModel.DataAnnotations;
using Lab6.Contracts;

namespace Lab6.Teacher;

/// <summary>Настройки экземпляра преподавателя, секция «Teacher».</summary>
public class TeacherOptions : IValidatableObject
{
    public const string SectionName = "Teacher";

    // Nullable, чтобы [Required] отличал «не задан» от первого значения enum.
    [Required]
    public Subject? Subject { get; set; }

    [Required]
    public Uri? HistoryUrl { get; set; }

    /// <summary>Адрес RabbitMQ вместе с логином и паролем, например amqp://user:pass@host:5672.</summary>
    [Required]
    public Uri? BrokerUrl { get; set; }

    /// <summary>Сколько ждать подтверждения брокера, прежде чем считать, что пара не записана.</summary>
    [Range(typeof(TimeSpan), "00:00:01", "00:01:00", ErrorMessage = "PublishTimeout должен быть от 1 секунды до 1 минуты")]
    public TimeSpan PublishTimeout { get; set; } = TimeSpan.FromSeconds(5);

    // Вызывается, только если прошли [Required], поэтому все значения уже есть.
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Enum.IsDefined(Subject!.Value))
        {
            yield return new ValidationResult($"Неизвестный предмет {Subject}", [nameof(Subject)]);
        }

        if (!HistoryUrl!.IsAbsoluteUri)
        {
            yield return new ValidationResult($"Адрес Истории должен быть полным: «{HistoryUrl}»", [nameof(HistoryUrl)]);
        }

        if (!BrokerUrl!.IsAbsoluteUri || BrokerUrl.Scheme is not ("amqp" or "amqps"))
        {
            yield return new ValidationResult($"Адрес RabbitMQ должен быть вида amqp://…: «{BrokerUrl}»", [nameof(BrokerUrl)]);
        }
    }
}
