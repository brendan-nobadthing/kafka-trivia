using System.Reflection;
using HotChocolate.Subscriptions;
using KafkaTriviaApi.Application.Commands;
using KafkaTriviaApi.Application.Models;
using MediatR;
using Serilog;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.Crosscutting;
using Streamiz.Kafka.Net.SerDes;
using Streamiz.Kafka.Net.Stream;
using Streamiz.Kafka.Net.Table;

namespace KafkaTriviaApi.KafkaStreams;

public static class StreamBuilders
{
    
    
    public static StreamBuilder BuildApplicationStreams(this StreamBuilder builder, 
        ITopicEventSender gqlSender,
        IMediator mediator)
    {
        var gameState = builder.GameStateStream();
        var gameStateTable = builder.GameStateChangedToTable(gameState);
        var gameNameLookup = builder.GameStateChangedToGameNameLookup(gameState);
        var gameParticipantsTable = builder.AddParticipant(gameStateTable);
        builder.StartGame(mediator);
        var questionsTable = builder.GameQuestionsTable();
        builder.HandleNextQuestion(gameStateTable);
        var gameParticipantStateTable = builder.GameParticipantState(gameState, gameParticipantsTable, questionsTable);
        
        // add some logging
        gameStateTable
            .ToStream()
            .Peek((k, v) => Log.Information("** Game State {@key}, {@Game}", k, v));
        
        gameParticipantsTable
            .ToStream()
            .Peek((k, v) => Log.Information("** Game Participants: {@GameParticipants}", v));

        // Game participants changed -> grahpql subscription
        gameParticipantsTable.ToStream()
            .Peek((k, v) => gqlSender.SendAsync("GameParticipantsChanged", new GameParticipants(v.FirstOrDefault()?.GameId ?? Guid.Empty, v)));
        
        // game participantStateTable -> graphQL subscription
        gameParticipantStateTable.ToStream()
            .Peek((k, v) => gqlSender.SendAsync("GameParticipantStateChanged", v));

        
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
    
    /// <summary>
    ///  Handle start game command. Fetch questions, push questions state, call NextQuestion 
    /// </summary>
    public static void StartGame(this StreamBuilder builder, IMediator mediator)
    {
        var stream = builder.Stream<string, StartGame>(
                KafkaStreamService.TopicNames.StartGame,
                new StringSerDes(),
                new JsonSerDes<StartGame>())
            .MapValuesAsync<GameQuestions>(async (kv, ctx) => 
                await mediator.Send(new FetchQuestions(){GameId = kv.Value.GameId}));
        
        // update GameQuestions state
        stream.To(KafkaStreamService.TopicNames.GameQuestions, new StringSerDes(), new JsonSerDes<GameQuestions>());
        
        // Send NextQuestion command
        stream.MapValues<NextQuestion>(v => new NextQuestion(){GameId = v.GameId})
            .To(KafkaStreamService.TopicNames.NextQuestion, new StringSerDes(), new JsonSerDes<NextQuestion>());
    }

    //save game questions to a table for later lookup
    public static IKTable<string, GameQuestions> GameQuestionsTable(this StreamBuilder builder)
    {
        return builder.Table(KafkaStreamService.TopicNames.GameQuestions, new StringSerDes(), new JsonSerDes<GameQuestions>(),
            InMemory.As<string, GameQuestions>());
    }

    // handle NextQuestionCommand. push update to core game state. Schedule the CloseQuestion command
    public static void HandleNextQuestion(this StreamBuilder builder, IKTable<string, Game> gameStateTable)
    {
        var nextQuestionStream = builder.Stream<string, NextQuestion>(
            KafkaStreamService.TopicNames.NextQuestion,
            new StringSerDes(),
            new JsonSerDes<NextQuestion>());

        nextQuestionStream.Join(gameStateTable,
            (nextQuestion, game) => game with { CurrentQuestionNumber = game.CurrentQuestionNumber + 1 })
            .To(KafkaStreamService.TopicNames.GameState);
            
        
        nextQuestionStream.MapValuesAsync<CloseQuestion>(async (kv, ctx) =>
        {
            await Task.Delay(10 * 1000);
            return new CloseQuestion() { GameId = kv.Value.GameId };
        })
        .To(KafkaStreamService.TopicNames.CloseQuestion, new StringSerDes(), new JsonSerDes<CloseQuestion>());
    }


    /// <summary>
    /// when active game state changes, merge state into records for each participant 
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="gameParticipantsTable"></param>
    /// <param name="gameQuestionsTable"></param>
    public static IKTable<string, GameParticipantState> GameParticipantState(this StreamBuilder builder,
        IKStream<string, Game> gameStateStream, 
        IKTable<string, List<GameParticipant>> gameParticipantsTable,
        IKTable<string, GameQuestions> gameQuestionsTable)
    {
        var activeGameStream = gameStateStream
            // filter to active games only
            .Filter((k, v) => v.GameState != GameState.LobbyOpen && v.GameState != GameState.Finished);

            var joined = activeGameStream
                .LeftJoin(gameQuestionsTable, (game, questions) => new { Game = game, Questions = questions })
                .LeftJoin(gameParticipantsTable, (prevJoin, participants) => new { prevJoin.Game, prevJoin.Questions, participants });

            var participantStates = joined.FlatMap<string, GameParticipantState>((k, v) =>
            {
                var question = v.Questions.Questions[v.Game.CurrentQuestionNumber ?? 0 - 1];
                var results = new List<KeyValuePair<string, GameParticipantState>>();
                foreach (var participant in v.participants)
                {
                    results.Add(new KeyValuePair<string, GameParticipantState>(participant.ParticipantId.ToString(),
                        new GameParticipantState(
                            participant,
                            v.Game,
                            question.QuestionText,
                            question.Answers)));
                }

                return results;
            });
            participantStates.To(KafkaStreamService.TopicNames.GameParticipantState);

            return builder.Table<string, GameParticipantState>(
                KafkaStreamService.TopicNames.GameParticipantState,
                new StringSerDes(), new JsonSerDes<GameParticipantState>());
    }
    
   
}