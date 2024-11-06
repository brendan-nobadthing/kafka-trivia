using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Serilog;

namespace KafkaTriviaApi.KafkaStreams;

public class KafkaInit(ProducerConfig producerConfig)
{
    
    public async Task InitTopics(CancellationToken cancellationToken = default)
    {
        using (var adminClient = new AdminClientBuilder(new AdminClientConfig
                   { BootstrapServers = producerConfig.BootstrapServers }).Build())
        {
            try
            {
                await adminClient.CreateTopicsAsync(new TopicSpecification[]
                {
                    new TopicSpecification() { Name = "game-state-changed", NumPartitions = 3 }
                });    
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not create topics");
            }
        }
    }
}