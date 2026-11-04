using Lab4.Simulation;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace Lab4.Hosting;

public class SimulationWorker : BackgroundService
{
    private static readonly TimeSpan DayInterval = TimeSpan.FromSeconds(5);

    private readonly SemesterService _semesterService;
    private readonly ConsoleReporter _reporter;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly StartMode _mode;

    public SimulationWorker(
        SemesterService semesterService,
        ConsoleReporter reporter,
        IHostApplicationLifetime lifetime,
        StartMode mode)
    {
        _semesterService = semesterService;
        _reporter = reporter;
        _lifetime = lifetime;
        _mode = mode;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await RunSemesterAsync(stoppingToken);
        }
        catch (Exception e) when (e is NpgsqlException or InvalidDataException)
        {
            _reporter.PrintDatabaseError(e);
            Environment.ExitCode = 1;
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            // Ошибка в самой программе: печатаем целиком, иначе из-за StopApplication хост не успеет её залогировать.
            _reporter.PrintUnexpectedError(e);
            Environment.ExitCode = 1;
        }
        finally
        {
            _lifetime.StopApplication();
        }
    }

    private async Task RunSemesterAsync(CancellationToken stoppingToken)
    {
        SavedSemester? saved = _mode == StartMode.New
            ? await _semesterService.StartNewAsync()
            : await _semesterService.ContinueUnfinishedAsync();

        if (saved is null)
        {
            _reporter.PrintNothingToContinue();
            Environment.ExitCode = 1;
            return;
        }

        Semester semester = saved.Semester;
        _reporter.PrintSemesterStart(semester, saved.Id);

        using var timer = new PeriodicTimer(DayInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            DayOutcome outcome = semester.PlayNextDay();
            await _semesterService.SaveDayAsync(saved, outcome);
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
