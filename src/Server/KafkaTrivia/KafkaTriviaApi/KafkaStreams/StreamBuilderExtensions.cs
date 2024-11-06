using System.Reflection;
using Streamiz.Kafka.Net;

namespace KafkaTriviaApi.KafkaStreams;

public static class StreamBuilderExtensions
{

    public static StreamBuilder AddFromAssembly(this StreamBuilder builder, Assembly? assembly = null)
    {
        if (assembly == null) assembly = Assembly.GetExecutingAssembly();

        var builderItemTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IStreamBuilderItem).IsAssignableFrom(t));
        foreach (var itemType in builderItemTypes)
        {
            var builderItem = Activator.CreateInstance(itemType, assembly) as IStreamBuilderItem;
            builderItem?.Build(builder);
        }

        return builder;
    }
}