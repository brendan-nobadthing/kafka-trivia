namespace KafkaTriviaApi.Application.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string entityType, string entityId) : base($"Failed to find {entityType} with id {entityId}")
    {
    }

    public NotFoundException(string entityType, string entityId, Exception innerException) : base($"Failed to find {entityType} with id {entityId}", innerException)
    {
    }
    
    public NotFoundException(string message) : base(message)
    {
    }
}