using Dapper;
using Lab4.Persistence.Records;
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
    public async Task<int> CreateAsync(SemesterRecord semester)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync();
        await using NpgsqlTransaction transaction = await connection.BeginTransactionAsync();

        int semesterId = await connection.ExecuteScalarAsync<int>(
            "INSERT INTO semesters (seed, random_draws) VALUES (@Seed, @RandomDraws) RETURNING id",
            semester,
            transaction);

        foreach (TeacherRuleRecord rule in semester.Rules)
        {
            await connection.ExecuteAsync(
                """
                INSERT INTO teacher_rules (semester_id, subject, kind, subject_a, subject_b)
                VALUES (@SemesterId, @Subject, @Kind, @SubjectA, @SubjectB)
                """,
                new { SemesterId = semesterId, rule.Subject, rule.Kind, rule.SubjectA, rule.SubjectB },
                transaction);
        }

        await transaction.CommitAsync();
        return semesterId;
    }

    public async Task SaveDayAsync(int semesterId, DayRecord day)
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
                day.Day,
                day.TotalPleasure,
                day.IsExpelled,
                day.GoToLesson,
                day.Asked,
                day.RandomDraws,
            });
    }

    /// <returns>Последний семестр, где студент не вылетел и не прошёл все дни, или null.</returns>
    public async Task<int?> FindUnfinishedAsync(int daysInSemester)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync();

        return await connection.QuerySingleOrDefaultAsync<int?>(
            """
            SELECT s.id
            FROM semesters s
            WHERE NOT EXISTS (
                SELECT 1 FROM days d
                WHERE d.semester_id = s.id AND (d.is_expelled OR d.day >= @daysInSemester))
            ORDER BY s.id DESC
            LIMIT 1
            """,
            new { daysInSemester });
    }

    /// <returns>Семестр с правилами и днями по порядку или null, если такого нет.</returns>
    public async Task<SemesterRecord?> LoadAsync(int semesterId)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync();

        SemesterRecord? semester = await connection.QuerySingleOrDefaultAsync<SemesterRecord>(
            "SELECT seed, random_draws AS RandomDraws FROM semesters WHERE id = @semesterId",
            new { semesterId });

        if (semester is null)
        {
            return null;
        }

        semester.Rules.AddRange(await connection.QueryAsync<TeacherRuleRecord>(
            """
            SELECT subject, kind, subject_a AS SubjectA, subject_b AS SubjectB
            FROM teacher_rules
            WHERE semester_id = @semesterId
            """,
            new { semesterId }));

        semester.Days.AddRange(await connection.QueryAsync<DayRecord>(
            """
            SELECT day, total_pleasure AS TotalPleasure, is_expelled AS IsExpelled,
                   go_to_lesson AS GoToLesson, asked, random_draws AS RandomDraws
            FROM days
            WHERE semester_id = @semesterId
            ORDER BY day
            """,
            new { semesterId }));

        return semester;
    }
}
