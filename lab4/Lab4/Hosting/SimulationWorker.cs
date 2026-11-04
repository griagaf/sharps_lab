using Lab4.Persistence;
using Lab4.Randomness;
using Lab4.Simulation;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace Lab4.Hosting;

public class SimulationWorker : BackgroundService
{
    private static readonly TimeSpan DayInterval = TimeSpan.FromSeconds(5);

    private readonly SemesterFactory _semesterFactory;
    private readonly SemesterRepository _repository;
    private readonly ConsoleReporter _reporter;
    private readonly IHostApplicationLifetime _lifetime;

    public SimulationWorker(
        SemesterFactory semesterFactory,
        SemesterRepository repository,
        ConsoleReporter reporter,
        IHostApplicationLifetime lifetime)
    {
        _semesterFactory = semesterFactory;
        _repository = repository;
        _reporter = reporter;
        _lifetime = lifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await RunSemesterAsync(stoppingToken);
        }
        catch (NpgsqlException e)
        {
            _reporter.PrintDatabaseError(e);
            Environment.ExitCode = 1;
        }
        finally
        {
            _lifetime.StopApplication();
        }
    }

    private async Task RunSemesterAsync(CancellationToken stoppingToken)
    {
        var random = new SystemRandomSource();
        Semester semester = _semesterFactory.CreateSemester(random);
        int semesterId = await _repository.CreateAsync(semester, random);
        _reporter.PrintSemesterStart(semester, semesterId);

        using var timer = new PeriodicTimer(DayInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            DayOutcome outcome = semester.PlayNextDay();
            await _repository.SaveDayAsync(semesterId, semester, outcome, random);
            _reporter.PrintDay(semester, outcome.SkippedSubjects);

            if (semester.IsExpelled)
            {
                _reporter.PrintExpelled(semester);
                return;
            }

            if (semester.IsFinished)
            {
                _reporter.PrintFinished(semester);
                return;
            }
        }
    }
}
