using System.Globalization;
using HotChocolate.Subscriptions;
using KafkaTriviaApi.Application.Commands;
using KafkaTriviaApi.Application.Models;
using MediatR;
using Serilog;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.SerDes;
using Streamiz.Kafka.Net.Stream;
using Streamiz.Kafka.Net.Table;
using Path = HotChocolate.Path;

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
        var answersByQuestionTable = builder.AnswersByQuestionTable();
        builder.UpdateCurrentAnswerStats(answersByQuestionTable, gameParticipantsTable, gameStateTable);
        var participantScoresTable = builder.CloseQuestionAndCalculateScores(gameStateTable, gameParticipantsTable, answersByQuestionTable, questionsTable);
        var gameParticipantStateTable = builder.GameParticipantState(gameState, gameParticipantsTable, questionsTable, participantScoresTable);
        
        // add some logging
        gameStateTable
            .ToStream()
            .Peek((k, v) => Log.Information("** Game State Table {@key}, {@Game}", k, v));
        
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
            .Peek((k, v) => Log.Information("* Handling Start Game {@GameId}", k));
            
            
        var questionsStream = stream.MapValuesAsync<GameQuestions>(
            async (kv, ctx) => 
                await mediator.Send(new FetchQuestions() { GameId = kv.Value.GameId }),
                    RetryPolicy.NewBuilder().NumberOfRetry(5).Build(),
                    new RequestSerDes<string, StartGame>(new StringSerDes(), new JsonSerDes<StartGame>()),
                    new ResponseSerDes<string, GameQuestions>(new StringSerDes(), new JsonSerDes<GameQuestions>())
            )
            .Peek((k, v) => Log.Information("* Got Questions {@Questions}", v));
        
        // update GameQuestions state
        questionsStream.To(KafkaStreamService.TopicNames.GameQuestions, new StringSerDes(), new JsonSerDes<GameQuestions>());
        
        // Send NextQuestion command
        questionsStream.MapValues<NextQuestion>(
                v => new NextQuestion(){GameId = v.GameId})
            .Peek((k,v) => Log.Information("* Sending NextQuestion {@NextQuestion}", v))
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
            new JsonSerDes<NextQuestion>())
            .Peek((k,v) => Log.Information("* Handle NextQuestion {@key}, {@NextQuestion}", k, v));

        var gameAfterNextQuestionStream = nextQuestionStream
            .LeftJoin(gameStateTable, (nextQuestion, game) =>
            {
                Log.Information("* Next question lookup game {@NextQuestion} {@Game}", nextQuestion, game);
                var isLastQuestion = game.CurrentQuestionNumber == 10;
                return game with
                {
                    GameState = isLastQuestion ? GameState.Finished : GameState.QuestionOpen,
                    CurrentQuestionNumber = isLastQuestion ? null : (game.CurrentQuestionNumber??0) + 1
                };
            })
            .Peek((k,v) => Log.Information("* Game after next question {@key}, {@Game}", k, v));
            
        gameAfterNextQuestionStream.To(KafkaStreamService.TopicNames.GameState, new StringSerDes(), new JsonSerDes<Game>());
        
        gameAfterNextQuestionStream
            .Filter((k,v) => v.GameState == GameState.QuestionOpen)
            .MapValuesAsync<CloseQuestion>(async (kv, ctx) =>
            {
                await Task.Delay(10 * 1000);
                return new CloseQuestion() { GameId = kv.Value.GameId };
            },
            RetryPolicy.NewBuilder().NumberOfRetry(5).Build(),
            new RequestSerDes<string, Game>(new StringSerDes(), new JsonSerDes<Game>()),
            new ResponseSerDes<string, CloseQuestion>(new StringSerDes(), new JsonSerDes<CloseQuestion>()))
            .To(KafkaStreamService.TopicNames.CloseQuestion, new StringSerDes(), new JsonSerDes<CloseQuestion>());
    }




    /// <summary>
    /// when active game state changes, merge state into records for each participant 
    /// </summary>
    public static IKTable<string, GameParticipantState> GameParticipantState(this StreamBuilder builder,
        IKStream<string, Game> gameStateStream,
        IKTable<string, List<GameParticipant>> gameParticipantsTable,
        IKTable<string, GameQuestions> gameQuestionsTable,
        IKTable<string, List<GameParticipantAnswerScore>> participantScoresTable)
    {
        
        var startedGameStream = gameStateStream
            .Peek((k, v) => Log.Information("* Game Change {@GameState} {@CurrentQuestionsNumber}", v.GameState, v.CurrentQuestionNumber))
            .Filter((k, v) => v.GameState != GameState.LobbyOpen);
        
            var joined = startedGameStream
                .LeftJoin(gameQuestionsTable, (game, questions) => new { Game = game, Questions = questions })
                .LeftJoin(gameParticipantsTable, (prevJoin, participants) => new { prevJoin.Game, prevJoin.Questions, participants })
                .Peek((k, v) => Log.Information("*building Game Participant State {@key} {@data}", k, v));
            
            var participantStates = joined.FlatMap<string, GameParticipantState>((k, v) =>
            {
                var isValidQuestionNumber = v.Game.CurrentQuestionNumber != null 
                                            && v.Game.CurrentQuestionNumber >= 1
                                            && v.Game.CurrentQuestionNumber <= v.Questions.Questions.Count;
                var question = isValidQuestionNumber ? v.Questions.Questions[(v.Game.CurrentQuestionNumber??1)-1] : null;
                
                var results = new List<KeyValuePair<string, GameParticipantState>>();
                foreach (var participant in v.participants)
                {
                    results.Add(new KeyValuePair<string, GameParticipantState>(participant.ParticipantId.ToString(),
                        new GameParticipantState(
                            participant,
                            v.Game,
                            question?.QuestionText ?? "",
                            question?.Answers ?? new List<string>())));
                }

                return results;
            });
            participantStates.To(KafkaStreamService.TopicNames.GameParticipantPartialState, new StringSerDes(), new JsonSerDes<GameParticipantState>());

            builder.Stream(KafkaStreamService.TopicNames.GameParticipantPartialState, new StringSerDes(), new JsonSerDes<GameParticipantState>())
                .LeftJoin(participantScoresTable, (gps, scores) => gps with { Scores = scores })
                .To(KafkaStreamService.TopicNames.GameParticipantState, new StringSerDes(), new JsonSerDes<GameParticipantState>());

            return builder.Table<string, GameParticipantState>(
                KafkaStreamService.TopicNames.GameParticipantState,
                new StringSerDes(), new JsonSerDes<GameParticipantState>());
    }



    public static IKTable<string, List<AnswerQuestion>> AnswersByQuestionTable(this StreamBuilder builder)
    {
        var answersByQuestionTable =  builder.Stream<string, AnswerQuestion>(
            KafkaStreamService.TopicNames.AnswerQuestion,
            new StringSerDes(),
            new JsonSerDes<AnswerQuestion>())
            .Peek((k,v) => Log.Information("* Handling Answer {@Answer}", v))
            .GroupBy((k, v) => $"{k}:{v.QuestionNumber}") // composite key to identify question under a game
            .Aggregate(
                () => new List<AnswerQuestion>(),
                (k, v, old) =>
                {
                    old.Add(v);
                    return old;
                },
                InMemory.As<string,List<AnswerQuestion>>(KafkaStreamService.TopicNames.AnswersByQuestionTable).WithValueSerdes<JsonSerDes<List<AnswerQuestion>>>()
            );
        return answersByQuestionTable;
    }

    public static void UpdateCurrentAnswerStats(this StreamBuilder builder, 
        IKTable<string, List<AnswerQuestion>> answersByQuestionTable, 
        IKTable<string, List<GameParticipant>> gameParticipantsTable,
        IKTable<string, Game> gameStateTable)
    {
        var answersStream = answersByQuestionTable
            .ToStream()
            .Peek((k, v) => Log.Information("* Answer -> Stats {@key} {@Answer}", k, v))
            .Map((k, v) =>
                KeyValuePair.Create(v.FirstOrDefault()!.GameId.ToString(), v)); // extract gameId as key for joining
        
        var joined = answersStream
            .LeftJoin(
                gameStateTable, 
                (answers, game) => new { Game = game, Answers = answers }, 
                new StreamTableJoinProps<string, List<AnswerQuestion>, Game>(new StringSerDes(), new JsonSerDes<List<AnswerQuestion>>(), new JsonSerDes<Game>()))
            .LeftJoin(gameParticipantsTable, (prevJoin, participants) => new { Answers = prevJoin.Answers, Game = prevJoin.Game, Participants = participants })
            .Peek((k,v) => Log.Information("Update Stats Joined Data {@JoinedData}", v));

        joined
            .MapValues(v => v.Game with { CurrentQuestionStats = $"{v.Answers.Count} of {v.Participants.Count}"})
            .Peek((k,v) => Log.Information("Answer -> stats Updating Game {@Game}", v))
            .To(KafkaStreamService.TopicNames.GameState, new StringSerDes(), new JsonSerDes<Game>());
    }

    public static IKTable<string, List<GameParticipantAnswerScore>> CloseQuestionAndCalculateScores(this StreamBuilder builder, 
        IKTable<string, Game> gameStateTable, 
        IKTable<string, List<GameParticipant>> gameParticipantsTable,
        IKTable<string, List<AnswerQuestion>> answersByQuestionTable,
        IKTable<string, GameQuestions> gameQuestionsTable)
    {
        var closeQuestionStream = builder.Stream<string, CloseQuestion>(
                KafkaStreamService.TopicNames.CloseQuestion,
                new StringSerDes(), new JsonSerDes<CloseQuestion>())
            .Peek((k, v) => Log.Information("Close Question {@Key}", v));
        
        // Calculate Scores, save to table by participant ID
        closeQuestionStream
            .LeftJoin(gameStateTable, (closeQuestion, game) => game)
            .Peek((k,v) => Log.Information("Score Join 1 {@Value}", v))
            .LeftJoin(gameParticipantsTable, (game, participants) => new { Game = game, Participants = participants })
            .Peek((k,v) => Log.Information("Score Join 2 {@Value}", v))
            .LeftJoin(gameQuestionsTable, (prevJoin, questions) => new GameParticipantsQuestions(prevJoin.Game, prevJoin.Participants, questions))
            
            .Peek((k,v) => Log.Information("Score Join 3 {@Value}", v))
            .Map((k, v) =>  KeyValuePair.Create(key: $"{v.Game.GameId}:{v.Game.CurrentQuestionNumber}", value: v)) // build the composite key required to join with answersByQuestionTable
            .Peek((k,v) => Log.Information("After Key Map {@value}", v))
            .LeftJoin(answersByQuestionTable, 
                (prevJoin, answers) => new GameParticipantsQuestionsAnswers(prevJoin.Game, prevJoin.Participants, prevJoin.Questions, answers),
                new StreamTableJoinProps<string, GameParticipantsQuestions, List<AnswerQuestion>>(new StringSerDes(), new JsonSerDes<GameParticipantsQuestions>(), new JsonSerDes<List<AnswerQuestion>>())
                )
            .Peek((k,v) => Log.Information("Score Join 4 {@Value}", v))
            .FlatMap((k, v) =>
            {
                Log.Information("* Score processing {@countAnswers} answers", v.Answers?.Count ?? 0);
                var answers = v.Answers ?? [];
                var results = new List<KeyValuePair<string, GameParticipantAnswerScore>>();
                var currentQuestion = v.Questions.Questions.FirstOrDefault(q => q.QuestionNumber == v.Game.CurrentQuestionNumber)!;
                var answersSortedByCorrectThenTime = answers
                    .Where(a => a.AnswerIndex == currentQuestion.CorrectAnswerIndex)
                    .OrderBy(a => a.TimestampUtc).ToList();
                answersSortedByCorrectThenTime.AddRange(answers
                    .Where( a => a.AnswerIndex != currentQuestion.CorrectAnswerIndex)
                    .OrderBy(a => a.TimestampUtc));    
                
                foreach (var participant in v.Participants)
                {
                    var participantAnswer = answers.OrderBy(a => a.TimestampUtc)
                        .FirstOrDefault(a => a.ParticipantId == participant.ParticipantId);
                    if (participantAnswer == null || participantAnswer.AnswerIndex != currentQuestion.CorrectAnswerIndex)
                    {
                        // Score is 0
                        Log.Information("Zero Score: {@ParticipantAnswer}, {@CurrentQuestion}", participantAnswer, currentQuestion);
                        results.Add(new KeyValuePair<string, GameParticipantAnswerScore>(participant.ParticipantId.ToString(), new GameParticipantAnswerScore(
                            v.Game.GameId, participant.ParticipantId, v.Game.CurrentQuestionNumber??0, participantAnswer?.AnswerIndex, 0, currentQuestion.CorrectAnswerIndex)));
                        continue;
                    };
                    // every correct answer earns num players + num players you beat this round
                    Log.Information("** Score Calc: {@ParticipantsCount} {@SortedAnswers}", v.Participants.Count, answersSortedByCorrectThenTime);
                    var score = v.Participants.Count +
                                (v.Participants.Count - (answersSortedByCorrectThenTime.IndexOf(participantAnswer)+1));
                    results.Add(new KeyValuePair<string, GameParticipantAnswerScore>(participant.ParticipantId.ToString(), new GameParticipantAnswerScore(
                        v.Game.GameId, participant.ParticipantId, v.Game.CurrentQuestionNumber??0, participantAnswer?.AnswerIndex, score ,currentQuestion.CorrectAnswerIndex)));
                }
                return results;
            })
            // explicitly writing to a topic to force serdes settings
            .To(KafkaStreamService.TopicNames.GameParticipantAnswers, new StringSerDes(), new JsonSerDes<GameParticipantAnswerScore>());
            
            // aggregate to participant -> scores lookup table
            var gameParticipantAnswersTable = builder.Stream(KafkaStreamService.TopicNames.GameParticipantAnswers, new StringSerDes(), new JsonSerDes<GameParticipantAnswerScore>())
                .GroupByKey()
                .Aggregate(() => new List<GameParticipantAnswerScore>(),
                    (k, v, old) =>
                    {
                        old.Add(v);
                        return old;
                    },
                    InMemory.As<string,List<GameParticipantAnswerScore>>(KafkaStreamService.TopicNames.GameParticipantAnswersTable)
                        .WithValueSerdes<JsonSerDes<List<GameParticipantAnswerScore>>>()
                );
            
        
        // push Gamestate change to QuestionResult
        closeQuestionStream.LeftJoin(gameStateTable, (closeQuestion, game) => game with { GameState = GameState.QuestionResult})
            .To(KafkaStreamService.TopicNames.GameState, new StringSerDes(), new JsonSerDes<Game>());
        
        // Schedule Next question
        closeQuestionStream
            .MapValuesAsync<NextQuestion>(async (kv, ctx) =>
                {
                    await Task.Delay(4 * 1000);
                    return new NextQuestion() { GameId = kv.Value.GameId };
                },
                RetryPolicy.NewBuilder().NumberOfRetry(5).Build(),
                new RequestSerDes<string, CloseQuestion>(new StringSerDes(), new JsonSerDes<CloseQuestion>()),
                new ResponseSerDes<string, NextQuestion>(new StringSerDes(), new JsonSerDes<NextQuestion>()))
            .To(KafkaStreamService.TopicNames.NextQuestion, new StringSerDes(), new JsonSerDes<NextQuestion>());

        return gameParticipantAnswersTable;
    }


}

/// <summary>
/// types created to help serdes for a big join result
/// </summary>
///
public record GameParticipantsQuestions(
    Game Game,
    List<GameParticipant> Participants,
    GameQuestions Questions
);
public record GameParticipantsQuestionsAnswers(
    Game Game,
    List<GameParticipant> Participants,
    GameQuestions Questions,
    List<AnswerQuestion> Answers
);
