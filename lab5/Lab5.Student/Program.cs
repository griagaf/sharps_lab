using System.Text;
using Lab5.Contracts;
using Lab5.Student;
using Lab5.Student.Hosting;
using Lab5.Student.Http;
using Lab5.Student.Simulation;
using Lab5.Student.Students;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

Console.OutputEncoding = Encoding.UTF8;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    // appsettings.json лежит рядом с exe, а запускать могут из любой папки.
    ContentRootPath = AppContext.BaseDirectory,
});

builder.Services.AddOptions<StudentOptions>()
    .BindConfiguration(StudentOptions.SectionName)
    .ValidateDataAnnotations();

builder.Services.AddSingleton<ISkipStrategy, EvenDaysStrategy>();

foreach (Subject subject in Subjects.All)
{
    builder.Services.AddHttpClient(HttpTeachers.GetClientName(subject), (services, http) =>
        http.BaseAddress = services.GetRequiredService<IOptions<StudentOptions>>().Value.Teachers[subject]);
}

builder.Services.AddSingleton<ITeachers, HttpTeachers>();
builder.Services.AddSingleton<SemesterSessionFactory>();
builder.Services.AddSingleton<ConsoleReporter>();

builder.Services.AddHostedService<SimulationWorker>();
builder.Services.AddHostedService<KeyPressWorker>();

IHost host = builder.Build();
ConsoleReporter reporter = host.Services.GetRequiredService<ConsoleReporter>();

try
{
    _ = host.Services.GetRequiredService<IOptions<StudentOptions>>().Value;
}
catch (OptionsValidationException e)
{
    reporter.PrintInvalidSettings(e.Failures);
    return 1;
}
// Значение не разобралось в нужный тип, например DayInterval=abc.
catch (InvalidOperationException e)
{
    reporter.PrintInvalidSettings([e.Message]);
    return 1;
}

host.Run();

return Environment.ExitCode;
