using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Serilog;

namespace KafkaTriviaApi.KafkaStreams;

public class KafkaInit(ProducerConfig producerConfig)
{
    
    public async Task InitTopics(CancellationToken cancellationToken = default)
    {
        using var adminClient = new AdminClientBuilder(new AdminClientConfig
            { BootstrapServers = producerConfig.BootstrapServers }).Build();
        try
        {
            var topics = new List<string>
            {
                KafkaStreamService.TopicNames.AddParticipant,
                KafkaStreamService.TopicNames.GameState,
                KafkaStreamService.TopicNames.OpenGamesByNameTable,
                KafkaStreamService.TopicNames.GameParticipantsTable,
                KafkaStreamService.TopicNames.GameStateTable,
                KafkaStreamService.TopicNames.StartGame,
                KafkaStreamService.TopicNames.GameQuestions, 
                KafkaStreamService.TopicNames.NextQuestion, 
                KafkaStreamService.TopicNames.CloseQuestion,
                KafkaStreamService.TopicNames.AnswerQuestion,
                KafkaStreamService.TopicNames.AnswersByQuestionTable,
                KafkaStreamService.TopicNames.GameParticipantPartialState,
                KafkaStreamService.TopicNames.GameParticipantState,
                KafkaStreamService.TopicNames.GameParticipantAnswers,
                KafkaStreamService.TopicNames.GameParticipantAnswersTable
            };

            bool deleteTopics = false; // TODO - config
            if (deleteTopics)
            {
                Log.Warning("DELETE Existing Topics");
                await adminClient.DeleteTopicsAsync(topics);
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
            
            Log.Warning("Creating Kafka Topics");
            await adminClient.CreateTopicsAsync(topics.Select(t => new TopicSpecification()
            {
                Name = t.ToLower(),
                NumPartitions = 1
            }));
               
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Could not create topics");
        }
    }
}