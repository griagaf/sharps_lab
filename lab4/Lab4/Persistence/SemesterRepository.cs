using Dapper;
using Lab4.Domain;
using Lab4.Randomness;
using Lab4.Simulation;
using Lab4.Teachers;
using Npgsql;

namespace Lab4.Persistence;

public class SemesterRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public SemesterRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    /// <returns>Номер семестра в БД.</returns>
    public async Task<int> CreateAsync(Semester semester, SystemRandomSource random)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync();
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

        int semesterId = await connection.ExecuteScalarAsync<int>(
            "INSERT INTO semesters (seed) VALUES (@Seed) RETURNING id",
            new { random.Seed },
            transaction);

        foreach (Subject subject in Subjects.All)
        {
            (string kind, Subject? subjectA, Subject? subjectB) = DescribeRule(semester.TeacherRules[(int)subject]);

            await connection.ExecuteAsync(
                """
                INSERT INTO teacher_rules (semester_id, subject, kind, subject_a, subject_b)
                VALUES (@SemesterId, @Subject, @Kind, @SubjectA, @SubjectB)
                """,
                new
                {
                    SemesterId = semesterId,
                    Subject = subject.ToString(),
                    Kind = kind,
                    SubjectA = subjectA?.ToString(),
                    SubjectB = subjectB?.ToString(),
                },
                transaction);
        }

        await transaction.CommitAsync();
        return semesterId;
    }

    public async Task SaveDayAsync(int semesterId, Semester semester, DayOutcome outcome, SystemRandomSource random)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync();

        await connection.ExecuteAsync(
            """
            INSERT INTO days (semester_id, day, total_pleasure, is_expelled, go_to_lesson, asked, random_draws)
            VALUES (@SemesterId, @Day, @TotalPleasure, @IsExpelled, @GoToLesson, @Asked, @RandomDraws)
            """,
            new
            {
                SemesterId = semesterId,
                outcome.Day,
                semester.TotalPleasure,
                semester.IsExpelled,
                outcome.GoToLesson,
                outcome.Asked,
                RandomDraws = random.Draws,
            });
    }

    private static (string Kind, Subject? SubjectA, Subject? SubjectB) DescribeRule(ITeacherRule rule)
    {
        return rule switch
        {
            RandomRule => ("Random", null, null),
            CopyYesterdayRule copy => ("CopyYesterday", copy.SubjectA, null),
            XorYesterdayRule xor => ("XorYesterday", xor.SubjectA, xor.SubjectB),
            _ => throw new ArgumentException($"Правило {rule.GetType().Name} нельзя сохранить в БД"),
        };
    }
}
