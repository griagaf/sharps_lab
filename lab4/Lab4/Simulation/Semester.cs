using Lab4.Domain;
using Lab4.Students;
using Lab4.Teachers;

namespace Lab4.Simulation;

public class Semester
{
    public const int DaysInSemester = 100;
    public const int PleasureForSkip = 1;
    public const int PleasureForPie = PleasureForSkip;

    private readonly ITeacherRule[] _teacherRules;
    private readonly ISkipStrategy _strategy;
    private readonly StudentHistory _history = new StudentHistory();

    // В первый день считается, что вчера никого не спрашивали.
    private bool[] _askedYesterday = new bool[Subjects.Count];

    public Semester(ITeacherRule[] teacherRules, ISkipStrategy strategy)
    {
        if (teacherRules.Length != Subjects.Count)
        {
            throw new ArgumentException($"Нужно {Subjects.Count} правил, по одному на предмет");
        }

        _teacherRules = teacherRules;
        _strategy = strategy;
    }

    public IReadOnlyList<ITeacherRule> TeacherRules => _teacherRules;

    public int DaysPassed { get; private set; }

    public int TotalPleasure { get; private set; }

    public bool IsExpelled { get; private set; }

    public Subject ExpelledSubject { get; private set; }

    public bool IsFinished => IsExpelled || DaysPassed >= DaysInSemester;

    public double AveragePleasurePerDay => DaysPassed == 0 ? 0 : (double)TotalPleasure / DaysPassed;

    public DayOutcome PlayNextDay()
    {
        EnsureNotFinished();

        bool[] goToLesson = DecideDay(DaysPassed + 1);
        bool[] askedToday = AskAllTeachers();

        return ApplyDay(goToLesson, askedToday);
    }

    /// <summary>
    /// Повторяет уже прошедший день по сохранённым решениям студента и ответам преподавателей.
    /// Ни стратегию, ни преподавателей при этом не спрашивает — так семестр восстанавливается из БД.
    /// </summary>
    public DayOutcome ReplayDay(bool[] goToLesson, bool[] askedToday)
    {
        EnsureNotFinished();

        if (goToLesson.Length != Subjects.Count || askedToday.Length != Subjects.Count)
        {
            throw new ArgumentException($"Нужно по {Subjects.Count} значений, по одному на предмет");
        }

        return ApplyDay(goToLesson, askedToday);
    }

    private void EnsureNotFinished()
    {
        if (IsExpelled)
        {
            throw new InvalidOperationException("Студент уже вылетел");
        }

        if (DaysPassed >= DaysInSemester)
        {
            throw new InvalidOperationException("Семестр уже закончился");
        }
    }

    private DayOutcome ApplyDay(bool[] goToLesson, bool[] askedToday)
    {
        int day = DaysPassed + 1;
        var skippedSubjects = new List<Subject>();

        foreach (Subject subject in Subjects.All)
        {
            bool attended = goToLesson[(int)subject];
            bool asked = askedToday[(int)subject];

            _history.SaveLesson(day, subject, attended, asked);

            if (attended)
            {
                continue;
            }

            skippedSubjects.Add(subject);

            if (asked)
            {
                IsExpelled = true;
                ExpelledSubject = subject;
                TotalPleasure = 0;
                DaysPassed = day;
                return new DayOutcome(day, goToLesson, askedToday, skippedSubjects);
            }

            TotalPleasure += PleasureForSkip;
        }

        TotalPleasure += PleasureForPie;
        DaysPassed = day;
        _askedYesterday = askedToday;

        return new DayOutcome(day, goToLesson, askedToday, skippedSubjects);
    }

    private bool[] DecideDay(int day)
    {
        bool[] goToLesson = _strategy.DecideDay(day, _history);

        if (goToLesson.Length != Subjects.Count)
        {
            throw new InvalidOperationException($"Стратегия должна вернуть решение по {Subjects.Count} предметам");
        }

        return goToLesson;
    }

    // Все преподаватели смотрят на вчерашний день, поэтому сегодняшние ответы
    // собираются в новый массив, а не записываются сразу в _askedYesterday.
    private bool[] AskAllTeachers()
    {
        var askedToday = new bool[Subjects.Count];

        for (int i = 0; i < askedToday.Length; i++)
        {
            askedToday[i] = _teacherRules[i].ShouldAsk(_askedYesterday);
        }

        return askedToday;
    }
}
