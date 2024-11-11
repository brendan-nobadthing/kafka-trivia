using System.Collections.Immutable;
using HotChocolate.Subscriptions;
using Serilog;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.SerDes;
using Streamiz.Kafka.Net.Stream;

namespace KafkaTriviaApi.KafkaStreams;

public class KafkaStreamService(ITopicEventSender gqlSender): IHostedService
{
    private KafkaStream? stream;
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var config = new StreamConfig<StringSerDes, StringSerDes>();
        config.ApplicationId = "kafka-trivia";
        config.BootstrapServers = "localhost:9092";

        StreamBuilder builder = new StreamBuilder();
        builder.BuildApplicationStreams(gqlSender);
        
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
        public static string GameParticipantsTable => "game-participants-table";
        
    }
    
   

}