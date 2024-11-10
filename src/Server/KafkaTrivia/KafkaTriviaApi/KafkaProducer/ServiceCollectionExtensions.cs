
using Confluent.Kafka;
using KafkaTriviaApi.Application.Models;
using KafkaTriviaApi.KafkaStreams;

namespace KafkaTriviaApi.KafkaProducer;

public static class ServiceCollectionExtensions
{

    public static void AddKafkaProducers(this IServiceCollection services)
    {
        services.AddKafkaProducer<Game>(KafkaStreamService.TopicNames.GameState); 
        services.AddKafkaProducer<GameParticipant>(KafkaStreamService.TopicNames.AddParticipant); 
    }


    public static void AddKafkaProducer<T>(this IServiceCollection services, string topic) where T : class
    {
        services.AddTransient<IMessageSender<T>>(s => 
            new KafkaMessageSender<T>(s.GetRequiredService<ProducerConfig>(), topic));
    }
}