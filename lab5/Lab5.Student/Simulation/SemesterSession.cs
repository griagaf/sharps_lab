using Lab5.Contracts;

namespace Lab5.Student.Simulation;

/// <summary>Семестр, который студент проходит, обращаясь к преподавателям.</summary>
public class SemesterSession
{
    private readonly ITeachers _teachers;

    public SemesterSession(Semester semester, ITeachers teachers)
    {
        Semester = semester;
        _teachers = teachers;
    }

    /// <summary>По нему История отличает этот семестр от других.</summary>
    public Guid Id { get; } = Guid.NewGuid();

    public Semester Semester { get; }

    /// <summary>
    /// Студент решает, куда идти, и проходит все 6 пар дня.
    /// Следующий день начинается только после этого, поэтому к его началу вчерашний день уже весь в Истории.
    /// </summary>
    public async Task<DayOutcome> PlayNextDayAsync(CancellationToken cancellationToken)
    {
        int day = Semester.NextDay;
        bool[] goToLesson = Semester.DecideNextDay();

        // Внутри дня преподаватели смотрят только на вчера, поэтому пары можно проводить одновременно.
        // Task.WhenAll сохраняет порядок, так что asked[i] — ответ по предмету i.
        bool[] asked = await Task.WhenAll(Subjects.All.Select(subject =>
            _teachers.HoldLessonAsync(subject, new LessonRequest(Id, day, goToLesson[(int)subject]), cancellationToken)));

        return Semester.ApplyDay(goToLesson, asked);
    }
}
