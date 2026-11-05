using System.Text;
using Lab5.Student.Students;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.OutputEncoding = Encoding.UTF8;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<ISkipStrategy, EvenDaysStrategy>();

IHost host = builder.Build();
host.Run();
