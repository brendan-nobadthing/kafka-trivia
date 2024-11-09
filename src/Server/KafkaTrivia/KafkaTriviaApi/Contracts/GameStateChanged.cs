namespace KafkaTriviaApi.Contracts;

public record GameStateChanged(
    Guid GameId,
    string Name,
    GameState GameState,
    int? CurrentQuestionNumber,
    DateTime TimestampUtc);
    
    