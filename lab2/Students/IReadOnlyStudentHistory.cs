using Lab2.Domain;

namespace Lab2.Students;

public interface IReadOnlyStudentHistory
{
    /// <summary>Был ли студент на паре по предмету в указанный день.</summary>
    bool Attended(int day, Subject subject);

    /// <summary>
    /// Спросили ли по предмету в указанный день — известно только если Attended(day, subject) == true.
    /// Если информации еще нет то null
    /// </summary>
    bool? WasAsked(int day, Subject subject);
}
