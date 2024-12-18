using System.Diagnostics;
using System.Text.Json;
using Confluent.Kafka;
using Serilog;

namespace KafkaTriviaApi.KafkaProducer;

public class KafkaMessageSender<T>(ProducerConfig producerConfig, string topicName) : IDisposable, IMessageSender<T>
    where T : class
{
    private readonly IProducer<string, string> _producer = new ProducerBuilder<string, string>(producerConfig).Build();

    public async Task Send(string key, T message, CancellationToken cancellationToken = default)
    {
        var msgText = JsonSerializer.Serialize(message);
        Log.Information("Sending {@Type} {@key} to topic {@Topic}", message.GetType().Name, key, topicName);
        var sw = Stopwatch.StartNew();
        await _producer.ProduceAsync(topicName, new Message<string, string> { Key = key, Value = msgText }, cancellationToken);
        sw.Stop();
        Log.Information("Producer Took {Elapsed}ms", sw.ElapsedMilliseconds);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(10));
        _producer.Dispose();
    }
}