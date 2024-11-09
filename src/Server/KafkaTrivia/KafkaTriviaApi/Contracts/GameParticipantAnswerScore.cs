namespace KafkaTriviaApi.Contracts;

public record GameParticipantAnswerScore(
    Guid GameId,
    Guid ParticipantId,
    int QuestionNumber,
    int Score);