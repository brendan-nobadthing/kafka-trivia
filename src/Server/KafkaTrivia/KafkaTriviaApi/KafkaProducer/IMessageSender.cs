namespace KafkaTriviaApi.KafkaProducer;

public interface IMessageSender<in T> where T:class
{
    Task Send(string key, T message, CancellationToken cancellationToken = default);
}