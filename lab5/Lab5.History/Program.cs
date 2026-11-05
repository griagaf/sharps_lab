using Lab5.History;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IHistoryStore, InMemoryHistoryStore>();

WebApplication app = builder.Build();

app.MapHistoryEndpoints();

app.Run();
