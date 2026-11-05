using Lab5.Contracts;
using Lab5.History;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(json => ContractJson.Configure(json.SerializerOptions));
builder.Services.AddSingleton<IHistoryStore, InMemoryHistoryStore>();

WebApplication app = builder.Build();

app.MapHistoryEndpoints();

app.Run();
