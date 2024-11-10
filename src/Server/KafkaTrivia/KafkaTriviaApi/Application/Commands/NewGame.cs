using KafkaTriviaApi.Application.Exceptions;
using KafkaTriviaApi.Application.Models;
using KafkaTriviaApi.Application.Queries;
using KafkaTriviaApi.KafkaProducer;
using MediatR;

namespace KafkaTriviaApi.Application.Commands;

public class NewGame: IRequest<Guid>
{
    public string Name { get; set; } = string.Empty;
}


public class NewGameHandler(IMediator mediator, IMessageSender<Game> sender) : IRequestHandler<NewGame, Guid>
{
    public async Task<Guid> Handle(NewGame request, CancellationToken cancellationToken)
    {
        var existing = await mediator.Send(new GetOpenGameByName(){Name = request.Name}, cancellationToken);
        if (existing != null) throw new StateConflictException("A open game already exists with that name");
        
        var gameId = Guid.NewGuid();
        await sender.Send(gameId.ToString(), new Game(gameId, request.Name, GameState.LobbyOpen,  null, DateTime.UtcNow), cancellationToken);
        return gameId;
    }
}
