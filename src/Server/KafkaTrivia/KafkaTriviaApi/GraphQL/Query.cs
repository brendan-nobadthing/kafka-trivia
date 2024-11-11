using KafkaTriviaApi.Application.Models;
using KafkaTriviaApi.Application.Queries;
using MediatR;

namespace KafkaTriviaApi.GraphQL;

public class Query 
{
    public async Task<Game?> GetGame(
        [Service] IMediator mediator,
        Guid gameId)
    {
        return await mediator.Send(new GetGameById() { GameId = gameId });
    }
    
    public async Task<Game?> GetGameByName(
        [Service] IMediator mediator,
        string gameName)
    {
        return await mediator.Send(new GetOpenGameByName() { Name = gameName });
    }
    
    
}