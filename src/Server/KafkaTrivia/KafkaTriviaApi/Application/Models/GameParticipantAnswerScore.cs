namespace KafkaTriviaApi.Application.Models;

public record GameParticipantAnswerScore(
    Guid GameId,
    Guid ParticipantId,
    int QuestionNumber,
    int? AnswerIndex,
    int Score,
    int CorrectAnswerIndex);