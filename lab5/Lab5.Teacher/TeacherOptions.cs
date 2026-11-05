using System.ComponentModel.DataAnnotations;
using Lab5.Contracts;

namespace Lab5.Teacher;

/// <summary>Настройки экземпляра преподавателя, секция «Teacher».</summary>
public class TeacherOptions
{
    public const string SectionName = "Teacher";

    // Nullable, чтобы [Required] отличал «не задан» от первого значения enum.
    [Required]
    public Subject? Subject { get; set; }

    [Required]
    public Uri? HistoryUrl { get; set; }
}
