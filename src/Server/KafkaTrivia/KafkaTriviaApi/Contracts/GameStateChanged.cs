namespace KafkaTriviaApi.Contracts;

public record GameStateChanged(
    Guid GameId,
    String Name,
    GameState GameState,
    int? CurrentQuestionNumber,
    DateTime TimestampUtc);
    
    