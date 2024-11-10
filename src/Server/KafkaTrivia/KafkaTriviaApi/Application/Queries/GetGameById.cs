using KafkaTriviaApi.Application.Models;
using KafkaTriviaApi.KafkaStreams;
using MediatR;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.State;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace KafkaTriviaApi.Application.Queries;

public class GetGameById : IRequest<Game?>
{
    public  Guid GameId { get; set; }
}

public class GetGameByIdHandler(KafkaStreamService kss) : IRequestHandler<GetGameById, Game?>
{
    public async Task<Game?> Handle(GetGameById request, CancellationToken cancellationToken)
    {
        var store = kss.Stream!.Store(StoreQueryParameters.FromNameAndType(KafkaStreamService.TopicNames.GameStateTable,
            QueryableStoreTypes.KeyValueStore<string, Game>()));
        var result = store.Get(request.GameId.ToString());
        return result;
    }
}