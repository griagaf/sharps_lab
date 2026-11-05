using Lab5.Contracts;
using Lab5.Student.Simulation;
using Lab5.Student.Students;

namespace Lab5.Student.Hosting;

public class ConsoleReporter
{
    private readonly ISkipStrategy _strategy;

    public ConsoleReporter(ISkipStrategy strategy)
    {
        _strategy = strategy;
    }

    public void PrintSemesterStart(SemesterSession session)
    {
        Console.WriteLine();
        Console.WriteLine($"===== Начало семестра {session.Id} =====");
        Console.WriteLine($"Стратегия: {_strategy.Name}");
        Console.WriteLine("Правила преподавателей скрыты: docker compose -f lab5/docker-compose.yml logs");
        Console.WriteLine("Нажмите любую клавишу, чтобы остановить");
        Console.WriteLine();
    }

    public void PrintDay(Semester semester, DayOutcome outcome)
    {
        // Студент знает ответ только там, где был. Вопрос на прогуле виден как вылет.
        var askedWhileAttended = Subjects.All
            .Where(subject => outcome.GoToLesson[(int)subject] && outcome.Asked[(int)subject])
            .ToList();

        Console.WriteLine($"День {outcome.Day,3} | прогулял: {JoinNames(outcome.SkippedSubjects)}");
        Console.WriteLine($"         | спросили: {JoinNames(askedWhileAttended)}");
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

    public void PrintStoppedByUser()
    {
        Console.WriteLine();
        Console.WriteLine("Остановлено по нажатию клавиши");
    }

    public void PrintTeachersUnavailable(HttpRequestException e)
    {
        Console.Error.WriteLine();
        Console.Error.WriteLine($"Не удалось провести пары: {e.Message}");
        Console.Error.WriteLine("Запущены ли сервисы? docker compose -f lab5/docker-compose.yml up -d --build");
    }

    public void PrintUnexpectedError(Exception e)
    {
        Console.Error.WriteLine();
        Console.Error.WriteLine($"Непредвиденная ошибка: {e}");
    }

    private static string JoinNames(IEnumerable<Subject> subjects)
    {
        List<string> names = subjects.Select(Subjects.GetName).ToList();

        return names.Count == 0 ? "ничего" : string.Join(", ", names);
    }
}
