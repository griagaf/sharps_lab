using Lab6.Contracts;
using Lab6.Student.Students;

namespace Lab6.Student.Simulation;

/// <summary>
/// Семестр глазами студента: решения по стратегии, что он видел на парах, удовольствие и вылет.
/// Как преподаватели решают спросить, семестр не знает — их ответы приходят в <see cref="ApplyDay"/>.
/// </summary>
public class Semester
{
    public const int DaysInSemester = 100;
    public const int PleasureForSkip = 1;
    public const int PleasureForPie = PleasureForSkip;

    private readonly ISkipStrategy _strategy;
    private readonly StudentHistory _history = new StudentHistory();

    public Semester(ISkipStrategy strategy)
    {
        _strategy = strategy;
    }

    public int DaysPassed { get; private set; }

    public int NextDay => DaysPassed + 1;

    public int TotalPleasure { get; private set; }

    public bool IsExpelled { get; private set; }

    public Subject ExpelledSubject { get; private set; }

    public bool IsFinished => IsExpelled || DaysPassed >= DaysInSemester;

    public double AveragePleasurePerDay => DaysPassed == 0 ? 0 : (double)TotalPleasure / DaysPassed;

    /// <returns>Решение на следующий день: для каждого предмета — идти (true) или прогулять.</returns>
    public bool[] DecideNextDay()
    {
        EnsureNotFinished();

        bool[] goToLesson = _strategy.DecideDay(NextDay, _history);

        if (goToLesson.Length != Subjects.Count)
        {
            throw new InvalidOperationException($"Стратегия должна вернуть решение по {Subjects.Count} предметам");
        }

        return goToLesson;
    }

    /// <summary>Проводит следующий день по решениям студента и ответам преподавателей.</summary>
    public DayOutcome ApplyDay(bool[] goToLesson, bool[] asked)
    {
        EnsureNotFinished();

        if (goToLesson.Length != Subjects.Count || asked.Length != Subjects.Count)
        {
            throw new ArgumentException($"Нужно по {Subjects.Count} значений, по одному на предмет");
        }

        int day = NextDay;
        var skippedSubjects = new List<Subject>();

        foreach (Subject subject in Subjects.All)
        {
            bool attended = goToLesson[(int)subject];
            bool wasAsked = asked[(int)subject];

            _history.SaveLesson(day, subject, attended, wasAsked);

            if (attended)
            {
                continue;
            }

            skippedSubjects.Add(subject);

            if (wasAsked)
            {
                IsExpelled = true;
                ExpelledSubject = subject;
                TotalPleasure = 0;
                DaysPassed = day;
                return new DayOutcome(day, goToLesson, asked, skippedSubjects);
            }

            TotalPleasure += PleasureForSkip;
        }

        TotalPleasure += PleasureForPie;
        DaysPassed = day;

        return new DayOutcome(day, goToLesson, asked, skippedSubjects);
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
}
