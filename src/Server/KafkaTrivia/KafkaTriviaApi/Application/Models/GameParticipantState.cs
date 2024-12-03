namespace KafkaTriviaApi.Application.Models;

/// <summary>
/// Collect All Game state data required for a single participant
/// </summary>
public record GameParticipantState(
    GameParticipant Participant,
    Game Game,
    string CurrentQuestion,
    IList<string> Answers,
    IList<GameParticipantAnswerScore>? Scores = null
    //int[] PrevScores
    );
    
    