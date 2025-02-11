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
using Squadtalk.Data.Repositories;
using Squadtalk.Services;

namespace Squadtalk.Signalr;

public partial class AppHub
{
    private ITextChatClient TextGroup(string groupName) => Clients.Group(groupName);

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

    [HubMethodName(HubMethods.MarkMessageSeen)]
    public async Task MarkMessageSeen(GroupId groupId, MessageId messageId, GroupRepository groupRepository)
    {
        await groupRepository.MarkMessageSeenAsync(groupId, UserId, messageId);
    }

    [HubMethodName(HubMethods.IsTyping)]
    public async Task IsTyping(GroupId groupId)
    {
        var userId = UserId;
        var shouldUpdate = await _connectionManager.SetUserIsTypingAsync(groupId, userId);

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

        if (shouldUpdate)
        {
            await Clients.OthersInGroup(groupId).UserStoppedTyping(groupId, userId);
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
            return HubResult.Error;
        }

        var group = await groupRepository.GetGroupAsync(groupId);
        var addingParticipant = group?.Participants.FirstOrDefault(x => x.UserId == UserId);
        if (group is null || addingParticipant is null)
        {
            return HubResult.Error;
        }

        if (group.ChatType != ChatType.GroupChat)
        {
            return HubResult.Error;
        }

        if (!addingParticipant.CanAddNewMembers())
        {
            return HubResult.Error;
        }

        var users = await _userRepository.GetUserListAsync(friendIds);

        var existingUserIds = group.Participants.Select(x => x.UserId).ToHashSet();
        var newUsers = users
            .Where(user => !existingUserIds.Contains(user.Id))
            .ToList();

        if (newUsers.Count == 0)
        {
            return HubResult.Ok;
        }

        var addedParticipants = newUsers.Select(x => x.ToGroupParticipant(group, addingParticipant.User));
        foreach (var newParticipant in addedParticipants)
        {
            group.Participants.Add(newParticipant);
        }

        if (!await groupRepository.UpdateGroupAsync(group))
        {
            return HubResult.Error;
        }

        var dto = group.ToDto();
        await NotifyNewGroupParticipantsAsync(dto, friendIds);
        await TextGroup(groupId).GroupParticipantsChanged(dto);

        return HubResult.Ok;
    }

    [HubMethodName(HubMethods.ChangeGroupName)]
    public async Task<HubResult> ChangeGroupName(
        GroupId groupId, string? newName, GroupRepository groupRepository, SystemMessageService systemMessageService)
    {
        if (newName?.Length > IFormValidator.MaximumGroupNameLength)
        {
            return HubResult.Error;
        }

        var group = await groupRepository.GetGroupAsync(groupId);
        var participant = group?.Participants.FirstOrDefault(x => x.UserId == UserId);
        if (group is null || participant is null)
        {
            return HubResult.Error;
        }

        if (group.ChatType != ChatType.GroupChat)
        {
            return HubResult.Error;
        }

        if (!participant.CanChangeGroupNameAndImage())
        {
            return HubResult.Unauthorized;
        }

        if (group.CustomName == newName)
        {
            return HubResult.Ok;
        }

        group.CustomName = string.IsNullOrWhiteSpace(newName) ? null : newName;
        if (!await groupRepository.UpdateGroupAsync(group))
        {
            return HubResult.Error;
        }

        await TextGroup(groupId).GroupNameChanged(groupId, group.CustomName);
        await systemMessageService.SendChannelNameChangedMessageAsync(participant.User, groupId, group.CustomName);
        return HubResult.Ok;
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

    [HubMethodName(HubMethods.ChangeUserRole)]
    public async Task<HubResult> ChangeUserRole(GroupId groupId, UserId targetUser, GroupRole newRole, GroupRepository groupRepository)
    {
        var userId = UserId;
        if (userId == targetUser)
        {
            return HubResult.Error;
        }

        var set = await GetGroupActorAndSubjectAsync(groupId, userId, targetUser, groupRepository);
        if (!set.IsValid || set.Group.ChatType != ChatType.GroupChat)
        {
            return HubResult.Error;
        }

        var subject = set.Subject;
        if (!set.Actor.CanChangeRoleTo(subject, newRole))
        {
            return HubResult.Unauthorized;
        }

        subject.Role = newRole;
        if (!await groupRepository.UpdateGroupAsync(set.Group))
        {
            return HubResult.Error;
        }

        await TextGroup(groupId).ParticipantRoleChanged(groupId, subject.UserId, subject.Role);
        return HubResult.Ok;
    }

    [HubMethodName(HubMethods.KickUser)]
    public async Task<HubResult> KickUser(GroupId groupId, UserId userToKickId, GroupRepository groupRepository)
    {
        var userId = UserId;
        if (userId == userToKickId)
        {
            return HubResult.Error;
        }

        var set = await GetGroupActorAndSubjectAsync(groupId, userId, userToKickId, groupRepository);
        if (!set.IsValid || set.Group.ChatType != ChatType.GroupChat)
        {
            return HubResult.Error;
        }

        if (!set.Actor.CanKick(set.Subject))
        {
            return HubResult.Unauthorized;
        }

        var group = set.Group;
        if (!group.Participants.Remove(set.Subject) || !await groupRepository.UpdateGroupAsync(group))
        {
            return HubResult.Error;
        }

        await TextGroup(groupId).GroupParticipantsChanged(group.ToDto());
        return HubResult.Ok;
    }

    [HubMethodName(HubMethods.LeaveGroup)]
    public async Task<HubResult> LeaveGroup(GroupId groupId, GroupRepository groupRepository)
    {
        var group = await groupRepository.GetGroupAsync(groupId);
        var participant = group?.Participants.FirstOrDefault(x => x.UserId == UserId);
        if (group is null || participant is null)
        {
            return HubResult.Error;
        }

        if (!group.Participants.Remove(participant))
        {
            return HubResult.Error;
        }

        if (group.Participants.Count == 0)
        {
            await groupRepository.DeleteGroupAsync(group);
            return HubResult.Ok;
        }

        if (!await groupRepository.UpdateGroupAsync(group))
        {
            return HubResult.Error;
        }

        await TextGroup(groupId).GroupParticipantsChanged(group.ToDto());

        return HubResult.Ok;
    }

    [HubMethodName(HubMethods.DeleteGroup)]
    public async Task<HubResult> DeleteGroup(GroupId groupId, GroupRepository groupRepository)
    {
        var group = await groupRepository.GetGroupAsync(groupId);
        var participant = group?.Participants.FirstOrDefault(x => x.UserId == UserId);
        if (group is null || participant is null)
        {
            return HubResult.Error;
        }

        if (group.ChatType != ChatType.GroupChat)
        {
            return HubResult.Error;
        }

        if (!participant.CanDeleteGroup())
        {
            return HubResult.Unauthorized;
        }

        if (!await groupRepository.DeleteGroupAsync(group))
        {
            return HubResult.Error;
        }

        await TextGroup(groupId).GroupDeleted(groupId);

        return HubResult.Ok;
    }

    [HubMethodName(HubMethods.CreateGroup)]
    public async Task<GroupDto?> CreateGroup(
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

        var dto = group.ToDto();

        await NotifyNewGroupParticipantsAsync(dto, group.Participants.Select(x => x.UserId));

        if (group.ChatType != ChatType.DirectMessage)
        {
            await systemMessageService.SendGroupCreatedMessageAsync(creatingUser, group.Id);
        }

        return dto;
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
