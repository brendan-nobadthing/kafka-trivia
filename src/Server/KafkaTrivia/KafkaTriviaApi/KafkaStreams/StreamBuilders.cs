using System.Reflection;
using HotChocolate.Subscriptions;
using KafkaTriviaApi.Application.Models;
using Serilog;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.Crosscutting;
using Streamiz.Kafka.Net.SerDes;
using Streamiz.Kafka.Net.Stream;
using Streamiz.Kafka.Net.Table;

namespace KafkaTriviaApi.KafkaStreams;

public static class StreamBuilders
{
    
    
    public static StreamBuilder BuildApplicationStreams(this StreamBuilder builder, ITopicEventSender gqlSender)
    {
        var gameState = builder.GameStateStream();
        var gameStateTable = builder.GameStateChangedToTable(gameState);
        var gameNameLookup = builder.GameStateChangedToGameNameLookup(gameState);
        var gameParticipantsTable = builder.AddParticipant(gameStateTable);
        
        // add some logging
        gameStateTable
            .ToStream()
            .Peek((k, v) => Log.Information("** Game State {@key}, {@Game}", k, v));
        
        gameParticipantsTable
            .ToStream()
            .Peek((k, v) => Log.Information("** Game Participants: {@GameParticipants}", v));

        gameParticipantsTable.ToStream()
            .Peek((k, v) => gqlSender.SendAsync("GameParticipantsChanged", new GameParticipants(v.FirstOrDefault()?.GameId ?? Guid.Empty, v)));
        
        return builder;
    }


    public static IKStream<string, Game> GameStateStream(this StreamBuilder builder)
    {
        return builder.Stream<string, Game>(
            KafkaStreamService.TopicNames.GameState,
            new StringSerDes(),
            new JsonSerDes<Game>());
    }

    
    public static IKTable<string, Game?> GameStateChangedToGameNameLookup(this StreamBuilder builder, IKStream<string, Game> gameStateStream)
    {
        // project gamestatechanged into a lookup table keyed by name
        return gameStateStream
            .GroupBy((k, v) => v.Name)
            .Aggregate(
                () => new Game(Guid.Empty, string.Empty, GameState.LobbyOpen, null, DateTime.MinValue),
                (k, v, old) =>
                    v.GameState == GameState.LobbyOpen ? v : null, 
                InMemory.As<string, Game?>(KafkaStreamService.TopicNames.OpenGamesByNameTable).WithValueSerdes<JsonSerDes<Game?>>()
            );
    }
    
    
    public static IKTable<string, Game> GameStateChangedToTable(this StreamBuilder builder, IKStream<string, Game> gameStateStream)
    {
        return gameStateStream.GroupByKey()
            .Aggregate(
                () => new Game(Guid.Empty, string.Empty, GameState.LobbyOpen, null, DateTime.MinValue),
                (k, v, old) => v,
                InMemory.As<string, Game>( KafkaStreamService.TopicNames.GameStateTable).WithValueSerdes<JsonSerDes<Game>>()
            );
    }
    
    
    public static IKTable<string, List<GameParticipant>> AddParticipant(this StreamBuilder builder,
        IKTable<string, Game> gameStateTable)
    {
        var result = builder.Stream<string, GameParticipant>(
                    KafkaStreamService.TopicNames.AddParticipant,
                    new StringSerDes(),
                    new JsonSerDes<GameParticipant>())
                //.Peek((k, v) => Log.Information("Game Participant stream: {@Key} {@GameParticipant}", k, v))
                .GroupByKey()
                .Aggregate(
                    () => new List<GameParticipant>(),
                    (k, v, old) =>
                    {
                        old.Add(v);
                        return old;
                    },
                    InMemory.As<string,List<GameParticipant>>(KafkaStreamService.TopicNames.GameParticipantsTable).WithValueSerdes<JsonSerDes<List<GameParticipant>>>()
                );
            return result;
    }
}