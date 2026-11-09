using Lab6.Contracts;
using Lab6.History;
using Microsoft.Extensions.Options;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(json => ContractJson.Configure(json.SerializerOptions));
builder.Services.AddSingleton<IHistoryStore, InMemoryHistoryStore>();

builder.Services.AddOptions<HistoryOptions>()
    .BindConfiguration(HistoryOptions.SectionName)
    .ValidateDataAnnotations();

builder.Services.AddHostedService<LessonQueueConsumer>();

WebApplication app = builder.Build();

app.MapHistoryEndpoints();

try
{
    _ = app.Services.GetRequiredService<IOptions<HistoryOptions>>().Value;
}
catch (OptionsValidationException e)
{
    PrintInvalidSettings(e.Failures);
    return 1;
}
// Значение не разобралось в нужный тип, например ReadTimeout=abc.
catch (InvalidOperationException e)
{
    PrintInvalidSettings([e.Message]);
    return 1;
}

app.Run();

return Environment.ExitCode;

static void PrintInvalidSettings(IEnumerable<string> problems)
{
    Console.Error.WriteLine("Неверные настройки Истории (appsettings.json, переменные окружения или аргументы запуска):");

    foreach (string problem in problems)
    {
        Console.Error.WriteLine($"  {problem}");
    }
}
