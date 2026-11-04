using System.Text;
using Lab4.Hosting;
using Lab4.Randomness;
using Lab4.Simulation;
using Lab4.Students;
using Lab4.Teachers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

Console.OutputEncoding = Encoding.UTF8;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

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
