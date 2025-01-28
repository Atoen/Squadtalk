using Microsoft.AspNetCore.SignalR.Client;
using Shared.Data;
using Shared.Data.TypedIds;
using Shared.DTOs.Chat;
using Shared.Extensions;
using Shared.Signalr;
using Shared.Signalr.Clients;
using Squadtalk.Client.Data;
using Squadtalk.Client.Services.SignalR.Interfaces;

namespace Squadtalk.Client.Services.SignalR;

internal sealed partial class SignalrService : ISignalrTextService
{
    public event Action<MessageDto>? MessageReceived;
    public event Action<GroupId, string?>? ChannelNameChanged;

    public event Action<GroupDto>? ChannelParticipantsChanged;
    public event Func<GroupDto, Task>? AddedToChannel;
    public event Func<IEnumerable<GroupDto>, Task>? ChannelsReceived;

    public event Action<GroupId, UserId>? UserIsTyping;
    public event Action<GroupId, UserId>? UserStoppedTyping;

    public Task<SignalrResult> SendMessageAsync(string message, GroupId groupId, CancellationToken cancellationToken)
    {
        return SendAsync(HubMethods.SendMessage, message, groupId, cancellationToken);
    }

    public Task<SignalrResult<List<MessageDto>>> GetMessagePageAsync(GroupId groupId, TextChannelCursor cursor = default, CancellationToken cancellationToken = default)
    {
        return InvokeAsync<List<MessageDto>>(HubMethods.GetMessagePage, groupId, cursor, cancellationToken);
    }

    public Task<SignalrResult<GroupId?>> CreateChannelAsync(IEnumerable<UserId> participants, CancellationToken cancellationToken = default)
    {
        return InvokeAsync<GroupId?>(HubMethods.CreateChannel, participants.ToList(), cancellationToken);
    }

    public Task<SignalrResult<bool>> AddFriendsToGroupAsync(GroupId groupId, IEnumerable<UserId> friends, CancellationToken cancellationToken = default)
    {
        return InvokeAsync<bool>(HubMethods.AddFriendsToChannel, groupId, friends.ToList(), cancellationToken);
    }

    public Task<SignalrResult<bool>> ChangeChannelNameAsync(GroupId groupId, string? newName, CancellationToken cancellationToken = default)
    {
        return InvokeAsync<bool>(HubMethods.ChangeChannelName, newName, cancellationToken);
    }

    public Task<SignalrResult> UserIsTypingAsync(GroupId groupId, CancellationToken cancellationToken = default)
    {
        return SendAsync(HubMethods.IsTyping, groupId, cancellationToken);
    }

    public Task<SignalrResult> UserStoppedTypingAsync(GroupId groupId, CancellationToken cancellationToken = default)
    {
        return SendAsync(HubMethods.StoppedTyping, groupId, cancellationToken);
    }

    private void RegisterTextHandlers()
    {
        _connection.On<MessageDto>(nameof(ITextChatClient.ReceivedMessage), message =>
            MessageReceived?.Invoke(message));

        _connection.On<IList<GroupDto>>(nameof(ITextChatClient.ReceivedChannels), channels =>
            ChannelsReceived.TryInvoke(channels));

        _connection.On<GroupDto>(nameof(ITextChatClient.AddedToChannel), channel =>
            AddedToChannel.TryInvoke(channel));

        _connection.On<GroupDto>(nameof(ITextChatClient.ChannelParticipantsChanged), channel =>
            ChannelParticipantsChanged?.Invoke(channel));

        _connection.On<GroupId, string?>(nameof(ITextChatClient.ChannelNameChanged), (channelId, newName) =>
            ChannelNameChanged?.Invoke(channelId, newName));

        _connection.On<GroupId, UserId>(nameof(ITextChatClient.UserIsTyping), (channelId, userId) =>
            UserIsTyping?.Invoke(channelId, userId));

        _connection.On<GroupId, UserId>(nameof(ITextChatClient.UserStoppedTyping), (channelId, userId) =>
            UserStoppedTyping?.Invoke(channelId, userId));
    }
}
