using Shared.Data.TypedIds;
using Shared.Signalr.Clients;
using Squadtalk.Data;
using Squadtalk.Repositories;

namespace Squadtalk.Signalr.Hubs;

partial class AppHub
{
    private ITextChatClient TextGroup(string groupName) => Clients.Group(groupName);
    private ITextChatClient TextClient(string connectionId) => Clients.Client(connectionId);
    private ITextChatClient TextCaller => Clients.Caller;

    public async Task SendMessage(string message, ChannelId channelId, MessageRepository messageRepository)
    {
        var participant = await GetChannelParticipantAsync(channelId);
        if (participant is null)
        {
            return;
        }

        var addedMessage = await messageRepository.AddMessageAsync(
            participant, message, channelId, cancellationToken: Context.ConnectionAborted);

        if (addedMessage is not null)
        {
            await TextGroup(channelId).ReceivedMessage(addedMessage.ToDto());
        }
    }
}
