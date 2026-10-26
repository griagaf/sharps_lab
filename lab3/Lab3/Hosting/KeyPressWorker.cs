using Microsoft.Extensions.Hosting;

namespace Lab3.Hosting;

public class KeyPressWorker : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMilliseconds(100);

    private readonly ConsoleReporter _reporter;
    private readonly IHostApplicationLifetime _lifetime;

    public KeyPressWorker(ConsoleReporter reporter, IHostApplicationLifetime lifetime)
    {
        _reporter = reporter;
        _lifetime = lifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Если ввод перенаправлен (например, из файла), клавиатуры нет и слушать нечего.
        if (Console.IsInputRedirected)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            if (Console.KeyAvailable)
            {
                Console.ReadKey(intercept: true);
                _reporter.PrintStoppedByUser();
                _lifetime.StopApplication();
                return;
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }
}
