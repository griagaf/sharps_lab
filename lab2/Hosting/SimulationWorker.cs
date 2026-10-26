using Lab2.Domain;
using Lab2.Simulation;
using Microsoft.Extensions.Hosting;

namespace Lab2.Hosting;

public class SimulationWorker : BackgroundService
{
    private static readonly TimeSpan DayInterval = TimeSpan.FromSeconds(5);

    private readonly SemesterFactory _semesterFactory;
    private readonly ConsoleReporter _reporter;
    private readonly IHostApplicationLifetime _lifetime;

    public SimulationWorker(SemesterFactory semesterFactory, ConsoleReporter reporter, IHostApplicationLifetime lifetime)
    {
        _semesterFactory = semesterFactory;
        _reporter = reporter;
        _lifetime = lifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Semester semester = StartSemester(1);

        using var timer = new PeriodicTimer(DayInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            List<Subject> skippedSubjects = semester.PlayNextDay();
            _reporter.PrintDay(semester, skippedSubjects);

            if (semester.IsExpelled)
            {
                _reporter.PrintExpelled(semester);
                _lifetime.StopApplication();
                return;
            }

            if (semester.IsFinished)
            {
                _reporter.PrintSemesterFinished(semester);
                semester = StartSemester(semester.Number + 1);
            }
        }
    }

    private Semester StartSemester(int number)
    {
        Semester semester = _semesterFactory.CreateSemester(number);
        _reporter.PrintSemesterStart(semester);
        return semester;
    }
}
