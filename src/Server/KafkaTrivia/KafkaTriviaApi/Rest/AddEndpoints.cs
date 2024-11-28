using KafkaTriviaApi.Application.Commands;
using KafkaTriviaApi.Application.Models;
using KafkaTriviaApi.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace KafkaTriviaApi.Rest;

public static class Endpoints
{

    public static WebApplication AddEndpoints(this WebApplication app)
    {
        app.AddGetGame();
        app.AddCheckName();
        app.AddNewGame();
        app.MapAddParticipant();
        return app;
    }
    
    public static WebApplication AddGetGame(this WebApplication app)
    {
        app.MapGet("/Game/{id}", async (
                [FromRoute] Guid id,
                [FromServices] IMediator mediator) =>
            {
                var existing = await mediator.Send(new GetGameById() { GameId = id });
                return existing;
            })
            .WithName("GetGame")
            .Produces<Game?>()
            .WithOpenApi();
        return app;
    }

    public static WebApplication AddCheckName(this WebApplication app)
    {
        app.MapGet("/CheckName/{name}", async (
            [FromRoute] string name,
            [FromServices] IMediator mediator) =>
            {
                var existing = await mediator.Send(new GetOpenGameByName() { Name = name });
                return existing;
            })
        .WithName("CheckName")
        .Produces<Game?>()
        .WithOpenApi();
        return app;
    }

    
    public static WebApplication AddNewGame(this WebApplication app)
    {
        app.MapPost("/NewGame", async (
                [FromQuery] string name,
                [FromServices] IMediator mediator) =>
            {
                var gameId = await mediator.Send(new NewGame() { Name = name });
                return Results.Created($"/api/game/{gameId}", null);
            })
            .WithName("NewGame")
            .WithOpenApi();
        return app;
    }
    
    
    public static WebApplication MapAddParticipant(this WebApplication app)
    {
        app.MapPost("/AddParticipant", async (
                [FromBody] AddParticipant cmd,
                [FromServices] IMediator mediator) =>
            {
                var result = await mediator.Send(new AddParticipant()
                {
                    Name = cmd.Name, 
                    GameName = cmd.GameName,
                    Email = cmd.Email
                });
                return Results.Json(result);
            })
            .WithName("AddParticipant")
            .WithOpenApi();
        return app;
    }
    
    
    
}