namespace KafkaTriviaApi.Application.Models;

public record GameParticipants(
    Guid GameId,
    IList<GameParticipant> Participants
);