using Lab5.Contracts;
using Lab5.Teacher;
using Lab5.Teacher.History;
using Lab5.Teacher.Randomness;
using Lab5.Teacher.Rules;
using Microsoft.Extensions.Options;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(json => ContractJson.Configure(json.SerializerOptions));

builder.Services.AddOptions<TeacherOptions>()
    .BindConfiguration(TeacherOptions.SectionName)
    .ValidateDataAnnotations();

builder.Services.AddSingleton<IRandomSource, SystemRandomSource>();
builder.Services.AddSingleton<TeacherRuleGenerator>();

// Правило выбирается один раз при старте и остаётся у преподавателя, пока он запущен.
builder.Services.AddSingleton(services => services.GetRequiredService<TeacherRuleGenerator>().CreateRandomRule());

builder.Services.AddHttpClient<IHistoryClient, HttpHistoryClient>((services, http) =>
    http.BaseAddress = services.GetRequiredService<IOptions<TeacherOptions>>().Value.HistoryUrl);

builder.Services.AddTransient<Lecturer>();

// Ошибки отдаются как ProblemDetails: сбой Истории — 502, всё остальное — 500.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<HistoryExceptionHandler>();

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.MapLessonEndpoints();

TeacherOptions options;

try
{
    options = app.Services.GetRequiredService<IOptions<TeacherOptions>>().Value;
}
catch (OptionsValidationException e)
{
    PrintInvalidSettings(e.Failures);
    return 1;
}
// Значение не разобралось в нужный тип, например Subject=Chemistry.
catch (InvalidOperationException e)
{
    PrintInvalidSettings([e.Message]);
    return 1;
}

ITeacherRule rule = app.Services.GetRequiredService<ITeacherRule>();
app.Logger.LogInformation("Предмет «{Subject}», правило: {Rule}", Subjects.GetName(options.Subject!.Value), rule.Description);

app.Run();

return 0;

static void PrintInvalidSettings(IEnumerable<string> problems)
{
    Console.Error.WriteLine("Неверные настройки преподавателя (appsettings.json, переменные окружения или аргументы запуска):");

    foreach (string problem in problems)
    {
        Console.Error.WriteLine($"  {problem}");
    }
}
