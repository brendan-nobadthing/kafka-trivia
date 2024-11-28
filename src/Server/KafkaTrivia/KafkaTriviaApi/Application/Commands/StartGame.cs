using KafkaTriviaApi.Application.Exceptions;
using KafkaTriviaApi.Application.Models;
using KafkaTriviaApi.Application.Queries;
using KafkaTriviaApi.KafkaProducer;
using MediatR;

namespace KafkaTriviaApi.Application.Commands;

public class StartGame : IRequest
{
    public Guid GameId { get; set; }
}

public class StartGameHandler(IMediator mediator, IMessageSender<StartGame> sender): IRequestHandler<StartGame>
{
    public async Task Handle(StartGame request, CancellationToken cancellationToken)
    {
        var game = await mediator.Send(new GetGameById(){GameId = request.GameId}, cancellationToken);
        if (game == null) throw new NotFoundException("Game", request.GameId.ToString());
        if (game.GameState != GameState.LobbyOpen) throw new StateConflictException("Game {Id} is not in LOBBY_OPEN state and cannot be started");
        
        var gameId = Guid.NewGuid();
        await sender.Send(game.GameId.ToString(), request, cancellationToken);
    }
}