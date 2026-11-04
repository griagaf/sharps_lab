namespace Lab4.Persistence.Records;

// Строка таблицы teacher_rules.
public class TeacherRuleRecord
{
    public string Subject { get; init; } = "";

    public string Kind { get; init; } = "";

    public string? SubjectA { get; init; }

    public string? SubjectB { get; init; }
}
