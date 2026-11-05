using Lab5.Student.Simulation;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Lab5.Student.Hosting;

public class SimulationWorker : BackgroundService
{
    private readonly SemesterSessionFactory _sessionFactory;
    private readonly ConsoleReporter _reporter;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly TimeSpan _dayInterval;

    public SimulationWorker(
        SemesterSessionFactory sessionFactory,
        ConsoleReporter reporter,
        IHostApplicationLifetime lifetime,
        IOptions<StudentOptions> options)
    {
        _sessionFactory = sessionFactory;
        _reporter = reporter;
        _lifetime = lifetime;
        _dayInterval = options.Value.DayInterval;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await RunSemesterAsync(stoppingToken);
        }
        catch (HttpRequestException e)
        {
            _reporter.PrintTeachersUnavailable(e);
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
        SemesterSession session = _sessionFactory.StartNew();
        Semester semester = session.Semester;

        _reporter.PrintSemesterStart(session);

        using var timer = new PeriodicTimer(_dayInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            DayOutcome outcome = await session.PlayNextDayAsync(stoppingToken);
            _reporter.PrintDay(semester, outcome);

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
