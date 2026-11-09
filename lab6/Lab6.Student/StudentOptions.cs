using System.ComponentModel.DataAnnotations;
using Lab6.Contracts;

namespace Lab6.Student;

/// <summary>Настройки студента, секция «Student».</summary>
public class StudentOptions : IValidatableObject
{
    public const string SectionName = "Student";

    /// <summary>Сколько длится один день семестра.</summary>
    public TimeSpan DayInterval { get; set; }

    /// <summary>Адрес преподавателя каждого предмета.</summary>
    public Dictionary<Subject, Uri> Teachers { get; set; } = new Dictionary<Subject, Uri>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DayInterval <= TimeSpan.Zero)
        {
            yield return new ValidationResult("День должен длиться больше нуля", [nameof(DayInterval)]);
        }

        foreach (Subject subject in Subjects.All)
        {
            if (!Teachers.TryGetValue(subject, out Uri? url))
            {
                yield return new ValidationResult($"Не задан адрес преподавателя предмета {subject}", [nameof(Teachers)]);
            }
            else if (!url.IsAbsoluteUri)
            {
                yield return new ValidationResult($"Адрес преподавателя предмета {subject} должен быть полным: «{url}»", [nameof(Teachers)]);
            }
        }
    }
}
