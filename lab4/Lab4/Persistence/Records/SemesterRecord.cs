namespace Lab4.Persistence.Records;

// Строка таблицы semesters вместе с её правилами и днями.
public class SemesterRecord
{
    public int Seed { get; init; }

    public int RandomDraws { get; init; }

    public List<TeacherRuleRecord> Rules { get; } = [];

    public List<DayRecord> Days { get; } = [];
}
