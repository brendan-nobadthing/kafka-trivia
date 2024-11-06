using KafkaTriviaApi.Contracts;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.SerDes;
using Streamiz.Kafka.Net.Table;

namespace KafkaTriviaApi.KafkaStreams.Streams;

public class GameStateChangedToGameNameStore : IStreamBuilderItem
{
    public StreamBuilder Build(StreamBuilder builder)
    {
        // project LobbyOpen gamestatechanged into a lookup table keyed by name
        var gameNamesTable = builder.Stream<string, GameStateChanged>(
                "game-state-changed",
                new StringSerDes(),
                new JsonSerDes<GameStateChanged>())
            .Filter((k, v) => v.GameState == GameState.LobbyOpen)
            .GroupBy((k, v) => v.Name)
            .Aggregate(
                () => new GameStateChanged(Guid.Empty, string.Empty, GameState.LobbyOpen, null, DateTime.MinValue),
                (k, v, old) => old with
                {
                    Name = v.Name,
                    TimestampUtc = new List<DateTime>() { old.TimestampUtc, v.TimestampUtc }.Max()
                },
                InMemory.As<string, GameStateChanged>("game-name-store").WithValueSerdes<JsonSerDes<GameStateChanged>>()
            );
        return builder;
    }
}