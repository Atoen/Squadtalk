using Microsoft.AspNetCore.SignalR.Client;
using Shared.Data;
using Shared.Data.TypedIds;
using Shared.DTOs.Chat;
using Shared.Enums;
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

    public event Action<GroupDto>? GroupParticipantsChanged;
    public event Func<GroupDto, Task>? AddedToGroup;
    public event Func<IEnumerable<GroupDto>, Task>? ChannelsReceived;

    public event Action<GroupId, UserId>? UserIsTyping;
    public event Action<GroupId, UserId>? UserStoppedTyping;

    public event Action<GroupId, UserId, GroupRole>? ParticipantRoleChanged;
    public event Action<GroupId>? GroupDeleted;

    public Task<NetworkResult> SendMessageAsync(string message, GroupId groupId, CancellationToken cancellationToken)
    {
        return SendAsync(HubMethods.SendMessage, message, groupId, cancellationToken);
    }

    public Task<NetworkResult<List<MessageDto>>> GetMessagePageAsync(GroupId groupId, TextChannelCursor cursor = default, CancellationToken cancellationToken = default)
    {
        return InvokeAsync<List<MessageDto>>(HubMethods.GetMessagePage, groupId, cursor, cancellationToken);
    }

    public Task<NetworkResult<GroupId?>> CreateGroupAsync(IEnumerable<UserId> participants, CancellationToken cancellationToken = default)
    {
        return InvokeAsync<GroupId?>(HubMethods.CreateGroup, participants.ToList(), cancellationToken);
    }

    public Task<NetworkResult<HubResult>> AddFriendsToGroupAsync(GroupId groupId, IEnumerable<UserId> friends, CancellationToken cancellationToken = default)
    {
        return InvokeAsync<HubResult>(HubMethods.AddFriendsToGroup, groupId, friends.ToList(), cancellationToken);
    }

    public Task<NetworkResult<HubResult>> ChangeGroupNameAsync(GroupId groupId, string? newName, CancellationToken cancellationToken = default)
    {
        return InvokeAsync<HubResult>(HubMethods.ChangeGroupName, groupId, newName, cancellationToken);
    }

    public Task<NetworkResult> UserIsTypingAsync(GroupId groupId, CancellationToken cancellationToken = default)
    {
        return SendAsync(HubMethods.IsTyping, groupId, cancellationToken);
    }

    public Task<NetworkResult> UserStoppedTypingAsync(GroupId groupId, CancellationToken cancellationToken = default)
    {
        return SendAsync(HubMethods.StoppedTyping, groupId, cancellationToken);
    }

    public Task<NetworkResult<HubResult>> KickUserAsync(GroupId groupId, UserId userId, CancellationToken cancellationToken = default)
    {
        return InvokeAsync<HubResult>(HubMethods.KickUser, groupId, userId, cancellationToken);
    }

    public Task<NetworkResult<HubResult>> ChangeUserRoleAsync(GroupId groupId, UserId userId, GroupRole newRole, CancellationToken cancellationToken = default)
    {
        return InvokeAsync<HubResult>(HubMethods.ChangeUserRole, groupId, userId, newRole, cancellationToken);
    }

    public Task<NetworkResult<HubResult>> LeaveGroupAsync(GroupId groupId, CancellationToken cancellationToken = default)
    {
        return InvokeAsync<HubResult>(HubMethods.LeaveGroup, groupId, cancellationToken);
    }

    public Task<NetworkResult<HubResult>> DeleteGroupAsync(GroupId groupId, CancellationToken cancellationToken = default)
    {
        return InvokeAsync<HubResult>(HubMethods.DeleteGroup, groupId, cancellationToken);
    }

    public Task<NetworkResult> MarkLastSeenAsync(GroupId currentGroupId, GroupId? previousGroupId = null, CancellationToken cancellationToken = default)
    {
        return SendAsync(HubMethods.MarkLastSeen, currentGroupId, previousGroupId, cancellationToken);
    }

    private void RegisterTextHandlers()
    {
        _connection.On<MessageDto>(nameof(ITextChatClient.ReceivedMessage), message =>
            MessageReceived?.Invoke(message));

        _connection.On<IList<GroupDto>>(nameof(ITextChatClient.ReceivedGroups), channels =>
            ChannelsReceived.TryInvoke(channels));

        _connection.On<GroupDto>(nameof(ITextChatClient.AddedToGroup), channel =>
            AddedToGroup.TryInvoke(channel));

        _connection.On<GroupDto>(nameof(ITextChatClient.GroupParticipantsChanged), channel =>
            GroupParticipantsChanged?.Invoke(channel));

        _connection.On<GroupId, string?>(nameof(ITextChatClient.GroupNameChanged), (channelId, newName) =>
            ChannelNameChanged?.Invoke(channelId, newName));

        _connection.On<GroupId, UserId>(nameof(ITextChatClient.UserIsTyping), (channelId, userId) =>
            UserIsTyping?.Invoke(channelId, userId));

        _connection.On<GroupId, UserId>(nameof(ITextChatClient.UserStoppedTyping), (channelId, userId) =>
            UserStoppedTyping?.Invoke(channelId, userId));

        _connection.On<GroupId, UserId, GroupRole>(nameof(ITextChatClient.ParticipantRoleChanged), (grupId, userId, role) =>
            ParticipantRoleChanged?.Invoke(grupId, userId, role));

        _connection.On<GroupId>(nameof(ITextChatClient.GroupDeleted), groupId =>
            GroupDeleted?.Invoke(groupId));
    }
}
