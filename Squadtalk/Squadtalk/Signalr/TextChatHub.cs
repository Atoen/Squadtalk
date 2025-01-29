using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.SignalR;
using Shared.Data;
using Shared.Data.TypedIds;
using Shared.DTOs.Chat;
using Shared.Enums;
using Shared.Services;
using Shared.Signalr;
using Shared.Signalr.Clients;
using Squadtalk.Data;
using Squadtalk.Data.Entities;
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
        var participant = await GetParticipatingUserWithGroups(groupId);
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

    private static readonly List<MessageDto> Empty = [];

    [HubMethodName(HubMethods.GetMessagePage)]
    public async Task<List<MessageDto>> GetMessagePage(
        GroupId groupId, TextChannelCursor cursor, MessageRepository messageRepository)
    {
        var participant = await GetParticipatingUserWithGroups(groupId);
        if (participant is null)
        {
            return Empty;
        }

        var messages = await messageRepository.GetPageAsync(groupId, cursor, Context.ConnectionAborted);
        return messages.Select(x => x.ToDto()).ToList();
    }

    [HubMethodName(HubMethods.AddFriendsToGroup)]
    public async Task<HubResult> AddFriendsToGroup(
        GroupId groupId, List<UserId> friendIds, GroupRepository groupRepository)
    {
        if (friendIds.Count == 0)
        {
            return HubResult.Fail;
        }

        var group = await groupRepository.GetGroupAsync(groupId);
        var addingParticipant = group?.Participants.FirstOrDefault(x => x.UserId == UserId);
        if (group is null || addingParticipant is null)
        {
            return HubResult.Fail;
        }

        if (!addingParticipant.CanAddNewMembers())
        {
            return HubResult.Fail;
        }

        var users = await _userRepository.GetUserListAsync(friendIds);

        var existingUserIds = group.Participants.Select(x => x.UserId).ToHashSet();
        var newUsers = users
            .Where(user => !existingUserIds.Contains(user.Id))
            .ToList();

        if (newUsers.Count == 0)
        {
            return HubResult.Success;
        }

        var addedParticipants = newUsers.Select(x => x.ToGroupParticipant(group, addingParticipant.User));
        foreach (var newParticipant in addedParticipants)
        {
            group.Participants.Add(newParticipant);
        }

        if (!await groupRepository.UpdateGroupAsync(group))
        {
            return HubResult.Fail;
        }

        var dto = group.ToDto();
        await NotifyNewGroupParticipantsAsync(dto, friendIds);
        await Clients.User(addingParticipant.UserId.ToString()).GroupParticipantsChanged(dto);

        return HubResult.Success;
    }

    [HubMethodName(HubMethods.ChangeGroupName)]
    public async Task<HubResult> ChangeGroupName(
        GroupId groupId, string? newName, GroupRepository groupRepository, SystemMessageService systemMessageService)
    {
        if (newName?.Length > IFormValidator.MaximumGroupNameLength)
        {
            return HubResult.Fail;
        }

        var group = await groupRepository.GetGroupAsync(groupId);
        var participant = group?.Participants.FirstOrDefault(x => x.UserId == UserId);
        if (group is null || participant is null)
        {
            return HubResult.Fail;
        }

        if (!participant.CanChangeGroupNameAndImage())
        {
            return HubResult.Fail;
        }

        if (group.Name == newName)
        {
            return HubResult.Success;
        }

        group.Name = newName;
        if (!await groupRepository.UpdateGroupAsync(group))
        {
            return HubResult.Fail;
        }

        await TextGroup(groupId).ChannelNameChanged(groupId, group.Name);
        await systemMessageService.SendChannelNameChangedMessageAsync(participant.User, groupId, group.Name);
        return HubResult.Success;
    }

    private readonly record struct GroupActorAndSubject(Group? Group, GroupParticipant? Actor, GroupParticipant? Subject)
    {
        [MemberNotNullWhen(true, nameof(Group), nameof(Actor), nameof(Subject))]
        public bool IsValid => Group is not null && Actor is not null && Subject is not null;
    }

    private static async Task<GroupActorAndSubject> GetGroupActorAndSubjectAsync(GroupId groupId, UserId actorId, UserId subjectId, GroupRepository groupRepository)
    {
        var group = await groupRepository.GetGroupAsync(groupId);
        var actor = group?.Participants.FirstOrDefault(x => x.UserId == actorId);
        var subject = group?.Participants.FirstOrDefault(x => x.UserId == subjectId);
        return new GroupActorAndSubject(group, actor, subject);
    }

    [HubMethodName(HubMethods.PromoteUser)]
    public async Task<HubResult> PromoteUser(GroupId groupId, UserId userToPromoteId, GroupRole newRole, GroupRepository groupRepository)
    {
        var userId = UserId;
        if (userId == userToPromoteId)
        {
            return HubResult.Fail;
        }

        var set = await GetGroupActorAndSubjectAsync(groupId, userId, userToPromoteId, groupRepository);
        if (!set.IsValid)
        {
            return HubResult.Fail;
        }

        if (!set.Actor.CanPromoteTo(newRole, set.Subject))
        {
            return HubResult.Fail;
        }

        set.Subject.Role = newRole;
        if (!await groupRepository.UpdateGroupAsync(set.Group))
        {
            return HubResult.Fail;
        }

        // TODO: use more fine-grained method
        await TextGroup(groupId).GroupParticipantsChanged(set.Group.ToDto());
        return HubResult.Success;
    }

    [HubMethodName(HubMethods.DemoteUser)]
    public async Task<HubResult> DemoteUser(GroupId groupId, UserId userToDemoteId, GroupRole newRole, GroupRepository groupRepository)
    {
        var userId = UserId;
        if (userId == userToDemoteId)
        {
            return HubResult.Fail;
        }

        var set = await GetGroupActorAndSubjectAsync(groupId, userId, userToDemoteId, groupRepository);
        if (!set.IsValid)
        {
            return HubResult.Fail;
        }

        if (!set.Actor.CanDemoteTo(newRole, set.Subject))
        {
            return HubResult.Fail;
        }

        set.Subject.Role = newRole;
        if (!await groupRepository.UpdateGroupAsync(set.Group))
        {
            return HubResult.Fail;
        }

        // TODO: use more fine-grained method
        await TextGroup(groupId).GroupParticipantsChanged(set.Group.ToDto());
        return HubResult.Success;
    }

    [HubMethodName(HubMethods.KickUser)]
    public async Task<HubResult> KickUser(GroupId groupId, UserId userToKickId, GroupRepository groupRepository)
    {
        var userId = UserId;
        if (userId == userToKickId)
        {
            return HubResult.Fail;
        }

        var set = await GetGroupActorAndSubjectAsync(groupId, userId, userToKickId, groupRepository);
        if (!set.IsValid)
        {
            return HubResult.Fail;
        }

        if (!set.Actor.CanKick(set.Subject))
        {
            return HubResult.Fail;
        }

        var group = set.Group;
        if (!group.Participants.Remove(set.Subject) || !await groupRepository.UpdateGroupAsync(group))
        {
            return HubResult.Fail;
        }

        await TextGroup(groupId).GroupParticipantsChanged(group.ToDto());
        return HubResult.Fail;
    }

    [HubMethodName(HubMethods.DeleteGroup)]
    public async Task<HubResult> DeleteGroup(GroupId groupId, GroupRepository groupRepository)
    {
        var group = await groupRepository.GetGroupAsync(groupId);
        var participant = group?.Participants.FirstOrDefault(x => x.UserId == UserId);
        if (group is null || participant is null)
        {
            return HubResult.Fail;
        }

        if (!participant.CanDeleteGroup())
        {
            return HubResult.Fail;
        }

        if (!await groupRepository.DeleteGroupAsync(group))
        {
            return HubResult.Fail;
        }

        // TODO: notify clients

        return HubResult.Success;
    }

    [HubMethodName(HubMethods.CreateGroup)]
    public async Task<GroupId?> CreateGroup(
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

        var group = await groupRepository.CreateGroupAsync(creatingUser, participants, Context.ConnectionAborted);
        if (group is null)
        {
            return null;
        }

        await NotifyNewGroupParticipantsAsync(group.ToDto(), group.Participants.Select(x => x.UserId));

        if (group.ChatType != ChatType.DirectMessage)
        {
            await systemMessageService.SendGroupCreatedMessageAsync(creatingUser, group.Id);
        }

        return group.Id;
    }

    private async Task NotifyNewGroupParticipantsAsync(GroupDto groupDto, IEnumerable<UserId> participantsToNotify)
    {
        foreach (var participantId in participantsToNotify)
        {
            var userConnections = await _connectionManager.GetUserConnectionsAsync(participantId);
            foreach (var connection in userConnections)
            {
                await Groups.AddToGroupAsync(connection, groupDto.Id);
            }
        }

        await Clients.Groups(groupDto.Id).AddedToGroup(groupDto);
    }
}
