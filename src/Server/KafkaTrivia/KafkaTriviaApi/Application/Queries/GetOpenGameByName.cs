using KafkaTriviaApi.Application.Models;
using KafkaTriviaApi.KafkaStreams;
using MediatR;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.State;

namespace KafkaTriviaApi.Application.Queries;

/// <summary>
/// check whether there is currently a LobbyOpen game with the specified name
/// </summary>
public class GetOpenGameByName : IRequest<Game?>
{
    public string Name { get; set; } = string.Empty;
}



public class GetOpenGameByNameHandler(KafkaStreamService kss) : IRequestHandler<GetOpenGameByName, Game?>
{
    public async Task<Game?> Handle(GetOpenGameByName request, CancellationToken cancellationToken)
    {
        var store = kss.Stream!.Store(StoreQueryParameters.FromNameAndType(KafkaStreamService.TopicNames.OpenGamesByNameTable,
            QueryableStoreTypes.KeyValueStore<string, Game>()));
        var result = store.Get(request.Name!);
        return result;
    }
}