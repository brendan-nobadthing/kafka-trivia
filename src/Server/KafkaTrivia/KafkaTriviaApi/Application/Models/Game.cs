namespace KafkaTriviaApi.Application.Models;

public record Game(
    Guid GameId,
    string Name,
    GameState GameState,
    int? CurrentQuestionNumber,
    DateTime TimestampUtc,
    DateTime? GameStateTimestampUtc = null,  /* when GameState last changed for timeout display purposes */
    string? CurrentQuestionStats = null
    );


public enum GameState
{
    LobbyOpen = 1,
    QuestionOpen = 3,
    QuestionResult = 4,
    Finished = 5
}
    
    