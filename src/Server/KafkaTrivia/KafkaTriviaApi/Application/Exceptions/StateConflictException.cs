namespace KafkaTriviaApi.Application.Exceptions;

public class StateConflictException(string message) : Exception(message);