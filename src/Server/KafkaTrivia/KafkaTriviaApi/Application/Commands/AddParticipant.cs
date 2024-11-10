using KafkaTriviaApi.Application.Exceptions;
using KafkaTriviaApi.Application.Models;
using KafkaTriviaApi.Application.Queries;
using KafkaTriviaApi.KafkaProducer;
using MediatR;

namespace KafkaTriviaApi.Application.Commands;

public class AddParticipant : IRequest
{
    public string GameName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid ParticipantId { get; set; }
}



public class AddParticipantHandler(IMediator mediator, IMessageSender<GameParticipant> sender) : IRequestHandler<AddParticipant>
{
    public async Task Handle(AddParticipant request, CancellationToken cancellationToken)
    {
        var game = await mediator.Send(new GetOpenGameByName() { Name = request.GameName }, cancellationToken);
        if (game is null) throw new NotFoundException("Game", request.GameName);
        
        await sender.Send(game.GameId.ToString(), 
            new GameParticipant(game.GameId, request.ParticipantId, request.Name, request.Email), 
            cancellationToken);
    }
}