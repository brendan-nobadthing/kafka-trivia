using System.Net.Sockets;
using System.Text.Json;
using Confluent.Kafka;
using KafkaTriviaApi;
using KafkaTriviaApi.Contracts;
using Microsoft.AspNetCore.Mvc;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.State;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(builder.Configuration.GetSection("producer").Get<ProducerConfig>()!);

var kss = new KafkaStreamService();
builder.Services.AddSingleton(kss);
builder.Services.AddHostedService(_ => kss);

var app = builder.Build();

// Configure the HTTP request pipeline.
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

        var store = kss.Stream!.Store(StoreQueryParameters.FromNameAndType("game-state-store",
            QueryableStoreTypes.KeyValueStore<string, GameStateChanged>()));

        foreach (var kvp in store.All())
        {
            Console.WriteLine($"queried name: {kvp.Value.Name}");
        }
        
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


app.Run();

