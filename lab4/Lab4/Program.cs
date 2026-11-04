using System.Text;
using DbUp.Engine;
using Lab4.Hosting;
using Lab4.Persistence;
using Lab4.Randomness;
using Lab4.Simulation;
using Lab4.Students;
using Lab4.Teachers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

Console.OutputEncoding = Encoding.UTF8;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    // appsettings.json лежит рядом с exe, а запускать могут из любой папки.
    ContentRootPath = AppContext.BaseDirectory,
});

string? connectionString = builder.Configuration.GetConnectionString("Semesters");

if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.Error.WriteLine("Не задана строка подключения ConnectionStrings:Semesters в appsettings.json");
    return 1;
}

DatabaseUpgradeResult migration = DatabaseMigrator.Migrate(connectionString);

if (!migration.Successful)
{
    Console.Error.WriteLine($"Не удалось подготовить БД: {migration.Error.Message}");
    Console.Error.WriteLine("Запущен ли postgres? (docker compose up -d)");
    return 1;
}

// Служебные сообщения хоста ("Application started..." и т.п.) мешают читать вывод.
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);

builder.Services.AddSingleton<IRandomSource, SystemRandomSource>();
builder.Services.AddSingleton<ISkipStrategy, EvenDaysStrategy>();
builder.Services.AddSingleton<TeacherRuleGenerator>();
builder.Services.AddSingleton<SemesterFactory>();
builder.Services.AddSingleton<ConsoleReporter>();

builder.Services.AddHostedService<SimulationWorker>();
builder.Services.AddHostedService<KeyPressWorker>();

IHost host = builder.Build();
host.Run();

return 0;
