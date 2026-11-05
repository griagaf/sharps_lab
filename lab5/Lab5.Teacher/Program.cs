using Lab5.Teacher.Randomness;
using Lab5.Teacher.Rules;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IRandomSource, SystemRandomSource>();
builder.Services.AddSingleton<TeacherRuleGenerator>();

// Правило выбирается один раз при старте и остаётся у преподавателя, пока он запущен.
builder.Services.AddSingleton(services => services.GetRequiredService<TeacherRuleGenerator>().CreateRandomRule());

WebApplication app = builder.Build();

app.Run();
