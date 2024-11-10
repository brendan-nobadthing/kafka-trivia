namespace KafkaTriviaApi.Application.Models;

public record Game(
    Guid GameId,
    string Name,
    GameState GameState,
    int? CurrentQuestionNumber,
    DateTime TimestampUtc);


public enum GameState
{
    LobbyOpen,
    QuestionsRequested,
    QuestionOpen,
    QuestionResult,
    Finished
}
    
    