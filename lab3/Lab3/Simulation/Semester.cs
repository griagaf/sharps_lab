using Lab3.Domain;
using Lab3.Students;
using Lab3.Teachers;

namespace Lab3.Simulation;

public class Semester
{
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

    public double AveragePleasurePerDay => DaysPassed == 0 ? 0 : (double)TotalPleasure / DaysPassed;

    /// <returns>Предметы, которые студент прогулял в этот день.</returns>
    public List<Subject> PlayNextDay()
    {
        if (IsExpelled)
        {
            throw new InvalidOperationException("Студент уже вылетел");
        }

        int day = DaysPassed + 1;
        bool[] goToLesson = DecideDay(day);
        bool[] askedToday = AskAllTeachers();
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
                return skippedSubjects;
            }

            TotalPleasure += PleasureForSkip;
        }

        TotalPleasure += PleasureForPie;
        DaysPassed = day;
        _askedYesterday = askedToday;

        return skippedSubjects;
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
