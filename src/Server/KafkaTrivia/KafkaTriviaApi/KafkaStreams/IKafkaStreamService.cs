using Confluent.Kafka.Admin;

namespace KafkaTriviaApi.KafkaStreams;

public interface IKafkaStreamService
{
    IKafkaStreamStores Stores { get; }

    ITopics Topics { get; }
}

public interface ITopics
{
    TopicSpecification GameStateChanged { get; }
}

public interface ITopic<TKey, TValue>
{
    string Name { get; }
    int? PartitionCount { get; }
}


public  interface IKafkaStreamStores
{
    
}

