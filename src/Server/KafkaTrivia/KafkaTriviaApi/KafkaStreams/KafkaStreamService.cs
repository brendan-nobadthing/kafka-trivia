using System.Collections.Immutable;
using HotChocolate.Subscriptions;
using KafkaTriviaApi.Application.Commands;
using MediatR;
using Serilog;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.Processors;
using Streamiz.Kafka.Net.SerDes;
using Streamiz.Kafka.Net.Stream;

namespace KafkaTriviaApi.KafkaStreams;

public class KafkaStreamService(ITopicEventSender gqlSender, IMediator mediator): IHostedService
{
    private KafkaStream? stream;
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var config = new StreamConfig<StringSerDes, StringSerDes>();
        config.ApplicationId = "kafka-trivia";
        config.BootstrapServers = "localhost:9092";
        config.LingerMs = 5;

        StreamBuilder builder = new StreamBuilder();
        builder.BuildApplicationStreams(gqlSender, mediator);
        
        Topology t = builder.Build();
        
        Log.Information("starting with stare dir {@StateDir}", config.StateDir);
        stream = new KafkaStream(t, config);
        await stream.StartAsync();
    }
    
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        stream?.Dispose();
        return Task.CompletedTask;
    }
    
    public KafkaStream? Stream => stream;


    public class TopicNames
    {
        public const string GameState = "game-state";
        public const string GameStateTable = "game-state-table";
        public const string OpenGamesByNameTable = "open-games-by-name";
        
        public const string AddParticipant = "add-participant";
        public const string GameParticipantsTable = "game-participants-table";
        public const string StartGame = "start-game";
        public const string GameQuestions = "game-questions";
        public const string NextQuestion = "next-question";
        public const string CloseQuestion = "close-question";
        public const string ParticipantAnswer = "participant-answer";
        public const string GameParticipantState = "game-participant-state";
    }
    
   

}