using Microsoft.AspNetCore.SignalR;
using Shared.Data;
using Shared.Data.TypedIds;
using Shared.DTOs.Chat;
using Shared.Enums;
using Shared.Signalr;
using Shared.Signalr.Clients;
using Squadtalk.Data;
using Squadtalk.Repositories;
using Squadtalk.Services;

namespace Squadtalk.Signalr;

public partial class AppHub
{
    private ITextChatClient TextGroup(string groupName) => Clients.Group(groupName);
    private ITextChatClient TextClient(string connectionId) => Clients.Client(connectionId);
    private ITextChatClient TextCaller => Clients.Caller;

    [HubMethodName(HubMethods.SendMessage)]
    public async Task SendMessage(string message, GroupId groupId, MessageRepository messageRepository)
    {
        var participant = await GetChannelParticipantAsync(groupId);
        if (participant is null)
        {
            return;
        }

        var addedMessage = await messageRepository.AddMessageAsync(
            participant, message, groupId, cancellationToken: Context.ConnectionAborted);

        var shouldUpdate = await _connectionManager.SetUserStoppedTyping(groupId, participant.Id);
        if (shouldUpdate)
        {
            await Clients.OthersInGroup(groupId).UserStoppedTyping(groupId, UserId);
        }

        if (addedMessage is not null)
        {
            await TextGroup(groupId).ReceivedMessage(addedMessage.ToDto());
        }
    }

    [HubMethodName(HubMethods.IsTyping)]
    public async Task IsTyping(GroupId groupId)
    {
        var userId = UserId;

        var shouldUpdate = await _connectionManager.SetUserIsTypingAsync(groupId, userId);

        _logger.LogInformation("User {Id} is typing on channel {ChannelId}. Should update: {State}", userId, groupId, shouldUpdate);

        if (shouldUpdate)
        {
            await Clients.OthersInGroup(groupId).UserIsTyping(groupId, userId);
        }
    }

    [HubMethodName(HubMethods.StoppedTyping)]
    public async Task StoppedTyping(GroupId groupId)
    {
        var userId = UserId;

        var shouldUpdate = await _connectionManager.SetUserStoppedTyping(groupId, userId);

        _logger.LogInformation("User {Id} stopped typing. Should update: {State}", userId, shouldUpdate);

        if (shouldUpdate)
        {
            await Clients.OthersInGroup(groupId).UserStoppedTyping(groupId, UserId);
        }
    }

    [HubMethodName(HubMethods.AddFriendsToChannel)]
    public async Task<bool> AddFriendsToGroup(
        GroupId groupId, List<UserId> friendIds, GroupRepository groupRepository)
    {
        if (friendIds.Count == 0)
        {
            return false;
        }

        var addingUser = await GetChannelParticipantAsync(groupId);
        if (addingUser is null)
        {
            return false;
        }

        var users = await _userRepository.GetUserListAsync(friendIds);

        var added = await groupRepository.AddUsersToGroupAsync(groupId, addingUser, users);
        if (!added)
        {
            return false;
        }

        var channel = await groupRepository.GetGroupAsync(groupId);
        if (channel is null)
        {
            return false;
        }

        var dto = channel.ToDto();

        await NotifyNewChannelParticipantsAsync(dto, friendIds);
        await Clients.User(addingUser.Id.ToString()).ChannelParticipantsChanged(dto);

        return true;
    }

    private static readonly List<MessageDto> Empty = [];

    [HubMethodName(HubMethods.GetMessagePage)]
    public async Task<List<MessageDto>> GetMessagePage(
        GroupId groupId, TextChannelCursor cursor, MessageRepository messageRepository)
    {
        var participant = await GetChannelParticipantAsync(groupId);
        if (participant is null)
        {
            return Empty;
        }

        var messages = await messageRepository.GetPageAsync(groupId, cursor, Context.ConnectionAborted);
        return messages.Select(x => x.ToDto()).ToList();
    }

    [HubMethodName(HubMethods.CreateChannel)]
    public async Task<GroupId?> CreateChannel(
        List<UserId> participantIds, GroupRepository groupRepository, SystemMessageService systemMessageService)
    {
        var creatingUserId = UserId;
        if (!participantIds.Contains(creatingUserId))
        {
            return null;
        }

        var participants = await _userRepository.GetUserListAsync(participantIds);
        var creatingUser = participants.FirstOrDefault(x => x.Id == creatingUserId);
        if (creatingUser is null)
        {
            return null;
        }

        var channel = await groupRepository.CreateGroupAsync(creatingUser, participants, Context.ConnectionAborted);
        if (channel is null)
        {
            return null;
        }

        await NotifyNewChannelParticipantsAsync(channel.ToDto(), channel.Participants.Select(x => x.UserId));

        // Don't send the system message for dms
        if (channel.ChatType != ChatType.DirectMessage)
        {
            await systemMessageService.SendChannelCreatedMessageAsync(creatingUser, channel.Id);
        }

        return channel.Id;
    }

    private async Task NotifyNewChannelParticipantsAsync(GroupDto groupDto, IEnumerable<UserId> participantsToNotify)
    {
        foreach (var participantId in participantsToNotify)
        {
            var userConnections = await _connectionManager.GetUserConnectionsAsync(participantId);
            foreach (var connection in userConnections)
            {
                await Groups.AddToGroupAsync(connection, groupDto.Id);
            }
        }

        await Clients.Groups(groupDto.Id).AddedToChannel(groupDto);
    }
}
