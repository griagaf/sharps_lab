using Lab2.Domain;

namespace Lab2.Students;

public class StudentHistory : IReadOnlyStudentHistory
{
    // Элемент с индексом 0 — первый день. Списки растут вместе с семестром.
    private readonly List<bool[]> _attended = new List<bool[]>();
    private readonly List<bool?[]> _asked = new List<bool?[]>();

    public void SaveLesson(int day, Subject subject, bool attended, bool asked)
    {
        while (_attended.Count < day)
        {
            _attended.Add(new bool[Subjects.Count]);
            _asked.Add(new bool?[Subjects.Count]);
        }

        _attended[day - 1][(int)subject] = attended;

        if (attended)
        {
            _asked[day - 1][(int)subject] = asked;
        }
    }

    public bool Attended(int day, Subject subject)
    {
        if (!IsSavedDay(day))
        {
            return false;
        }

        return _attended[day - 1][(int)subject];
    }

    public bool? WasAsked(int day, Subject subject)
    {
        if (!IsSavedDay(day))
        {
            return null;
        }

        return _asked[day - 1][(int)subject];
    }

    private bool IsSavedDay(int day)
    {
        return day >= 1 && day <= _attended.Count;
    }
}
