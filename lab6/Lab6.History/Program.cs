using Lab6.Contracts;
using Lab6.History;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(json => ContractJson.Configure(json.SerializerOptions));
builder.Services.AddSingleton<IHistoryStore, InMemoryHistoryStore>();

WebApplication app = builder.Build();

app.MapHistoryEndpoints();

app.Run();
