using KafkaTriviaApi.Application.Exceptions;
using KafkaTriviaApi.Application.Models;
using KafkaTriviaApi.Application.Queries;
using KafkaTriviaApi.KafkaProducer;
using MediatR;

namespace KafkaTriviaApi.Application.Commands;

public class NewGame: IRequest<GameParticipant>
{
    public string Name { get; set; } = string.Empty;
}


public class NewGameHandler(IMediator mediator, IMessageSender<Game> gameSender, IMessageSender<GameParticipant> participantSender) : IRequestHandler<NewGame, GameParticipant>
{
    public async Task<GameParticipant> Handle(NewGame request, CancellationToken cancellationToken)
    {
        var existing = await mediator.Send(new GetOpenGameByName(){Name = request.Name}, cancellationToken);
        if (existing != null) throw new StateConflictException("A open game already exists with that name");
        
        var gameId = Guid.NewGuid();
        await gameSender.Send(gameId.ToString(), new Game(gameId, request.Name, GameState.LobbyOpen,  null, DateTime.UtcNow), cancellationToken);
        
        // register game owner as a participant
        var participant = new GameParticipant(
            gameId, Guid.NewGuid(), "Owner", "", true);
        await participantSender.Send(participant.ParticipantId.ToString(), participant, cancellationToken);

        return participant;
    }
}
