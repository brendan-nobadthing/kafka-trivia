using KafkaTriviaApi.Application.Exceptions;
using KafkaTriviaApi.Application.Models;
using KafkaTriviaApi.Application.Queries;
using KafkaTriviaApi.KafkaProducer;
using MediatR;

namespace KafkaTriviaApi.Application.Commands;

public class AddParticipant : IRequest<GameParticipant>
{
    public string GameName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}



public class AddParticipantHandler(IMediator mediator, IMessageSender<GameParticipant> sender) : IRequestHandler<AddParticipant, GameParticipant>
{
    public async Task<GameParticipant> Handle(AddParticipant request, CancellationToken cancellationToken)
    {
        var participantId = Guid.NewGuid();
        var game = await mediator.Send(new GetOpenGameByName() { Name = request.GameName }, cancellationToken);
        if (game is null) throw new NotFoundException("Game", request.GameName);
        
        var participant = new GameParticipant(game.GameId, participantId, request.Name, "xxx");
        await sender.Send(game.GameId.ToString(), 
            participant, 
            cancellationToken);
        
        return participant;
    }
}