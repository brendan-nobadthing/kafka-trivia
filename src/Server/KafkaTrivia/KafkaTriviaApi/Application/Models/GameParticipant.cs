namespace KafkaTriviaApi.Application.Models;

public record GameParticipant(
    Guid GameId,
    Guid ParticipantId,
    string Name,
    string GravatarCode
    );