using System.Net.Sockets;
using System.Text.Json;
using Confluent.Kafka;
using KafkaTriviaApi;
using KafkaTriviaApi.Contracts;
using KafkaTriviaApi.KafkaStreams;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.State;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

builder.Services.AddSingleton(builder.Configuration.GetSection("producer").Get<ProducerConfig>()!);

var kss = new KafkaStreamService();
builder.Services.AddSingleton(kss);
builder.Services.AddHostedService(_ => kss);
builder.Services.AddSingleton<KafkaInit>();


var app = builder.Build();
// Configure the HTTP request pipeline.
app.UseSerilogRequestLogging();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapPost("/NewGame", async (
    [FromQuery] string name,
    [FromServices] ProducerConfig producerConfig,
    [FromServices] KafkaStreamService kss) =>
    {
        var gameId = Guid.NewGuid();

        var store = kss.Stream!.Store(StoreQueryParameters.FromNameAndType("game-name-store",
            QueryableStoreTypes.KeyValueStore<string, GameStateChanged>()));


        var existing = store.Get(name);
        if (existing != null) Log.Warning("Game {@name} already exists", existing.Name);
        
        var _producer = new ProducerBuilder<string, string>(producerConfig).Build();
        await _producer.ProduceAsync("game-state-changed", new Message<string, string>()
        {
            Key = gameId.ToString(),
            Value = JsonSerializer.Serialize(new GameStateChanged(gameId, name, GameState.LobbyOpen, null, DateTime.UtcNow))
        });
        return Results.Created($"/api/game/{Guid.NewGuid()}", null);
    })
    .WithName("NewGame")
    .WithOpenApi();


app.Services.GetService<KafkaInit>()!.InitTopics().GetAwaiter().GetResult();

app.Run();

