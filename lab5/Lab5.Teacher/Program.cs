using Lab5.Contracts;
using Lab5.Teacher;
using Lab5.Teacher.History;
using Lab5.Teacher.Randomness;
using Lab5.Teacher.Rules;
using Microsoft.Extensions.Options;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<TeacherOptions>()
    .BindConfiguration(TeacherOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<IRandomSource, SystemRandomSource>();
builder.Services.AddSingleton<TeacherRuleGenerator>();

// Правило выбирается один раз при старте и остаётся у преподавателя, пока он запущен.
builder.Services.AddSingleton(services => services.GetRequiredService<TeacherRuleGenerator>().CreateRandomRule());

builder.Services.AddHttpClient<IHistoryClient, HttpHistoryClient>((services, http) =>
    http.BaseAddress = services.GetRequiredService<IOptions<TeacherOptions>>().Value.HistoryUrl);

builder.Services.AddTransient<Lecturer>();

WebApplication app = builder.Build();

app.MapLessonEndpoints();

TeacherOptions options = app.Services.GetRequiredService<IOptions<TeacherOptions>>().Value;
ITeacherRule rule = app.Services.GetRequiredService<ITeacherRule>();
app.Logger.LogInformation("Предмет «{Subject}», правило: {Rule}", Subjects.GetName(options.Subject!.Value), rule.Description);

app.Run();
