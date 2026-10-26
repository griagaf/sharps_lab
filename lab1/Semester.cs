namespace Lab1;

public class Semester
{
    public const int DaysCount = 100;
    public const int PleasureForSkip = 1;
    public const int PleasureForPie = PleasureForSkip;

    private readonly ITeacherRule[] _teacherRules;
    private readonly ISkipStrategy _strategy;

    public Semester(ITeacherRule[] teacherRules, ISkipStrategy strategy)
    {
        if (teacherRules.Length != Subjects.Count)
        {
            throw new ArgumentException($"Нужно {Subjects.Count} правил, по одному на предмет");
        }

        _teacherRules = teacherRules;
        _strategy = strategy;
    }

    public SemesterResult Run()
    {
        var history = new StudentHistory();
        int totalPleasure = 0;

        // В первый день считается, что вчера никого не спрашивали.
        var askedYesterday = new bool[Subjects.Count];

        for (int day = 1; day <= DaysCount; day++)
        {
            bool[] goToLesson = DecideDay(day, history);
            bool[] askedToday = AskAllTeachers(askedYesterday);

            foreach (Subject subject in Subjects.All)
            {
                bool attended = goToLesson[(int)subject];
                bool asked = askedToday[(int)subject];

                history.SaveLesson(day, subject, attended, asked);

                if (!attended)
                {
                    if (asked)
                    {
                        return new SemesterResult(day, subject);
                    }

                    totalPleasure += PleasureForSkip;
                }
            }

            totalPleasure += PleasureForPie;
            askedYesterday = askedToday;
        }

        return new SemesterResult(totalPleasure, DaysCount);
    }

    private bool[] DecideDay(int day, IReadOnlyStudentHistory history)
    {
        bool[] goToLesson = _strategy.DecideDay(day, history);

        if (goToLesson.Length != Subjects.Count)
        {
            throw new InvalidOperationException($"Стратегия должна вернуть решение по {Subjects.Count} предметам");
        }

        return goToLesson;
    }

    // Все преподаватели смотрят на вчерашний день, поэтому сегодняшние ответы
    // собираются в новый массив, а не записываются сразу в askedYesterday.
    private bool[] AskAllTeachers(bool[] askedYesterday)
    {
        var askedToday = new bool[Subjects.Count];

        for (int i = 0; i < askedToday.Length; i++)
        {
            askedToday[i] = _teacherRules[i].ShouldAsk(askedYesterday);
        }

        return askedToday;
    }
}
