using KafkaTriviaApi.Application.Commands;
using KafkaTriviaApi.Application.Models;
using MediatR;
using Serilog;

namespace KafkaTriviaApi.GraphQL;

public class Mutation
{
    [UseMutationConvention]
    public async Task<GameParticipant> NewGame([Service] IMediator mediator, string name)
    {
        try
        {
            var participant = await mediator.Send(new NewGame() { Name = name });
            return participant;
        }
        catch (Exception ex)
        {
            Log.Error(ex, ex.Message);
            throw;
        }
    }
    
    [UseMutationConvention]
    public async Task<GameParticipant> AddParticipant([Service] IMediator mediator, string gameName, string displayName, string email)
    {
        var newParticipantId = Guid.NewGuid();
        var response = await mediator.Send(new AddParticipant()
        {
            Name = displayName, 
            GameName = gameName,
            Email = email,
        });
        return response;
    }
    
    [UseMutationConvention]
    public async Task<StartGameResponse> StartGame([Service] IMediator mediator, Guid gameId)
    {
        await mediator.Send(new StartGame(){GameId = gameId});
        return new StartGameResponse(){GameId = gameId};
    }
}

public class StartGameResponse
{
    public Guid GameId { get; set; }
}