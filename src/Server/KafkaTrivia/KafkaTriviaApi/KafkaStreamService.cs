using KafkaTriviaApi.Contracts;
using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.SerDes;
using Streamiz.Kafka.Net.Stream;
using Streamiz.Kafka.Net.Table;

namespace KafkaTriviaApi;

public class KafkaStreamService: IHostedService
{
    private KafkaStream? stream;
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var config = new StreamConfig<StringSerDes, StringSerDes>();
        config.ApplicationId = "kafka-trivia";
        config.BootstrapServers = "localhost:9092";

        StreamBuilder builder = new StreamBuilder();

        var gameStateTable = builder.GlobalTable<string, GameStateChanged>(
            "game-state-changed", 
            new StringSerDes(),
            new JsonSerDes<GameStateChanged>(),
            InMemory.As<string, GameStateChanged>("game-state-store").WithValueSerdes<JsonSerDes<GameStateChanged>>());
        
        
        Topology t = builder.Build();
        stream = new KafkaStream(t, config);
        
        await stream.StartAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        stream.Dispose();
        return Task.CompletedTask;
    }
    
    public KafkaStream? Stream => stream;
}