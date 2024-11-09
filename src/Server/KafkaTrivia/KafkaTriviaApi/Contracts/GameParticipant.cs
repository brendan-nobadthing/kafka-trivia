namespace KafkaTriviaApi.Contracts;

public record GameParticipant(
    Guid GameId,
    Guid ParticipantId,
    string Name,
    string GravatarCode
    );