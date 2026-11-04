using Lab4.Domain;
using Lab4.Simulation;
using Lab4.Students;

namespace Lab4.Hosting;

public class ConsoleReporter
{
    private readonly ISkipStrategy _strategy;

    public ConsoleReporter(ISkipStrategy strategy)
    {
        _strategy = strategy;
    }

    public void PrintSemesterStart(Semester semester, int semesterId)
    {
        Console.WriteLine();
        Console.WriteLine(semester.DaysPassed == 0
            ? $"===== Начало семестра #{semesterId} ====="
            : $"===== Продолжаем семестр #{semesterId} с дня {semester.DaysPassed + 1}, удовольствие {semester.TotalPleasure} =====");
        Console.WriteLine($"Стратегия: {_strategy.Name}");
        Console.WriteLine("Правила преподавателей:");

        foreach (Subject subject in Subjects.All)
        {
            Console.WriteLine($"  {Subjects.GetName(subject),-22} {semester.TeacherRules[(int)subject].Description}");
        }

        Console.WriteLine("Нажмите любую клавишу, чтобы остановить");
        Console.WriteLine();
    }

    public void PrintDay(Semester semester, List<Subject> skippedSubjects)
    {
        string skipped = skippedSubjects.Count == 0 ? "ничего" : JoinNames(skippedSubjects);

        Console.WriteLine($"День {semester.DaysPassed,3} | прогулял: {skipped}");
        Console.WriteLine($"         | удовольствие: {semester.TotalPleasure}, в среднем за день: {semester.AveragePleasurePerDay:F2}");
    }

    public void PrintExpelled(Semester semester)
    {
        Console.WriteLine();
        Console.WriteLine($"Студент вылетел на {semester.DaysPassed}-й день: прогулял «{Subjects.GetName(semester.ExpelledSubject)}», а там спросили");
        Console.WriteLine("Всё удовольствие за семестр сгорело");
    }

    public void PrintFinished(Semester semester)
    {
        Console.WriteLine();
        Console.WriteLine($"Семестр окончен: {semester.DaysPassed} дней, удовольствие {semester.TotalPleasure}, " +
                          $"в среднем за день {semester.AveragePleasurePerDay:F2}");
    }

    public void PrintDatabaseError(Exception e)
    {
        Console.Error.WriteLine();
        Console.Error.WriteLine($"Ошибка БД: {e.Message}");
    }

    public void PrintUnexpectedError(Exception e)
    {
        Console.Error.WriteLine();
        Console.Error.WriteLine($"Непредвиденная ошибка: {e}");
    }

    public void PrintNothingToContinue()
    {
        Console.Error.WriteLine("Незавершённых семестров нет. Начните новый: --new");
    }

    public void PrintStoppedByUser()
    {
        Console.WriteLine();
        Console.WriteLine("Остановлено по нажатию клавиши. Продолжить: --continue");
    }

    private static string JoinNames(List<Subject> subjects)
    {
        var names = new List<string>();

        foreach (Subject subject in subjects)
        {
            names.Add(Subjects.GetName(subject));
        }

        return string.Join(", ", names);
    }
}
