using HotChocolate.Subscriptions;
using KafkaTriviaApi.Application.Models;

namespace KafkaTriviaApi.GraphQL;

public class Subscription
{

    /// <summary>
    /// subscribe with filter
    /// as per https://github.com/ChilliCream/graphql-platform/issues/5579
    /// </summary>
    [Subscribe(With = nameof(SubscribeToGameParticipantsChanged))]
    public GameParticipants GameParticipantsChanged([EventMessage] GameParticipants update, Guid gameId) => update;
    public async IAsyncEnumerable<GameParticipants> SubscribeToGameParticipantsChanged(
        [Service] ITopicEventReceiver receiver, Guid gameId)
    {
        var stream = await receiver.SubscribeAsync<GameParticipants>("GameParticipantsChanged");
        await foreach (var update in stream.ReadEventsAsync())
        {
            if (update.GameId == gameId)
            {
                yield return update;
            }
        }
    }
    
    
    [Subscribe(With = nameof(SubscribeToGameParticipantStateChanged))]
    public GameParticipantState GameParticipantStateChanged([EventMessage] GameParticipantState state, Guid participantId) => state;
    public async IAsyncEnumerable<GameParticipantState> SubscribeToGameParticipantStateChanged(
        [Service] ITopicEventReceiver receiver, Guid participantId)
    {
        var stream = await receiver.SubscribeAsync<GameParticipantState>("GameParticipantStateChanged");
        await foreach (var update in stream.ReadEventsAsync())
        {
            if (update.Participant.ParticipantId == participantId)
            {
                yield return update;
            }
        }
    }
    
    
}