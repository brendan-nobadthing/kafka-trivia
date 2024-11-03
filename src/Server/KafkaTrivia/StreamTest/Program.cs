// See https://aka.ms/new-console-template for more information

using Streamiz.Kafka.Net;
using Streamiz.Kafka.Net.SerDes;
using Streamiz.Kafka.Net.Stream;

namespace StreamTest;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var config = new StreamConfig<StringSerDes, StringSerDes>();
        config.ApplicationId = "test-app";
        config.BootstrapServers = "localhost:9092";
        

        StreamBuilder builder = new StreamBuilder();

        builder.Stream<string, string>("game-state-changed")
            .FilterNot((k, v) => v.Contains("key1"))
            .To("test-output");

        Topology t = builder.Build();
        KafkaStream stream = new KafkaStream(t, config);
        

        Console.CancelKeyPress += (o, e) => {
            stream.Dispose();
        };

        await stream.StartAsync();
    }
}