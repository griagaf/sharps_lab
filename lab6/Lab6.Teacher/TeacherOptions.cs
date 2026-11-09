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

    // Вызывается, только если прошли [Required], поэтому оба значения уже есть.
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
    }
}
