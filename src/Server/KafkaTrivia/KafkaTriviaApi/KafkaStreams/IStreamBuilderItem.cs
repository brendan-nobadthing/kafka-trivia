using Streamiz.Kafka.Net;

namespace KafkaTriviaApi.KafkaStreams;

public interface IStreamBuilderItem
{
    StreamBuilder Build(StreamBuilder builder);
}