namespace KafkaTriviaApi.Application.Models;

public record Game(
    Guid GameId,
    string Name,
    GameState GameState,
    int? CurrentQuestionNumber,
    DateTime TimestampUtc);


public enum GameState
{
    LobbyOpen = 1,
    QuestionOpen = 3,
    QuestionResult = 4,
    Finished = 5
}
    
    