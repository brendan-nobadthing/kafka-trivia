using KafkaTriviaApi.Contracts;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.SerDes;
using Streamiz.Kafka.Net.Table;

namespace KafkaTriviaApi.KafkaStreams.Streams;

public class GameStateChangedToGameNameLookup : IStreamBuilderItem
{
    public StreamBuilder Build(StreamBuilder builder)
    {
        // project gamestatechanged into a lookup table keyed by name
        var gameNamesTable = builder.Stream<string, GameStateChanged>(
                "game-state-changed",
                new StringSerDes(),
                new JsonSerDes<GameStateChanged>())
            .Filter((k, v) => v.GameState == GameState.LobbyOpen)
            .GroupBy((k, v) => v.Name)
            .Aggregate(
                () => new GameStateChanged(Guid.Empty, string.Empty, GameState.LobbyOpen, null, DateTime.MinValue),
                (k, v, old) =>
                    v.TimestampUtc > old.TimestampUtc ? v: old,
        InMemory.As<string, GameStateChanged>("game-state-by-name").WithValueSerdes<JsonSerDes<GameStateChanged>>()
            );
        return builder;
    }
}