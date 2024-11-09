namespace KafkaTriviaApi.Contracts;

public record GameParticipantAnswer(
    Guid GameId,
    Guid ParticipantId,
    int QuestionNumber,
    int Answer,
    DateTime TimestampUtc);