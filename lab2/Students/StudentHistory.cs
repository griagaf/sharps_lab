using Lab2.Domain;
using Lab2.Simulation;

namespace Lab2.Students;

public class StudentHistory : IReadOnlyStudentHistory
{
    private readonly bool[,] _attended = new bool[Semester.DaysCount + 1, Subjects.Count];
    private readonly bool?[,] _asked = new bool?[Semester.DaysCount + 1, Subjects.Count];

    public void SaveLesson(int day, Subject subject, bool attended, bool asked)
    {
        _attended[day, (int)subject] = attended;

        if (attended)
        {
            _asked[day, (int)subject] = asked;
        }
    }

    public bool Attended(int day, Subject subject)
    {
        return _attended[day, (int)subject];
    }

    public bool? WasAsked(int day, Subject subject)
    {
        return _asked[day, (int)subject];
    }
}
