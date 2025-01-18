using Microsoft.AspNetCore.SignalR;
using Shared.Data;
using Shared.Data.TypedIds;
using Shared.DTOs.Chat;
using Shared.Signalr;
using Shared.Signalr.Clients;
using Squadtalk.Data;
using Squadtalk.Data.Entities;
using Squadtalk.Repositories;
using Squadtalk.Services;

namespace Squadtalk.Signalr;

partial class AppHub
{
    private ITextChatClient TextGroup(string groupName) => Clients.Group(groupName);
    private ITextChatClient TextClient(string connectionId) => Clients.Client(connectionId);
    private ITextChatClient TextCaller => Clients.Caller;

    [HubMethodName(HubMethods.SendMessage)]
    public async Task SendMessage(string message, ChannelId channelId, MessageRepository messageRepository)
    {
        var participant = await GetChannelParticipantAsync(channelId);
        if (participant is null)
        {
            return;
        }

        var addedMessage = await messageRepository.AddMessageAsync(
            participant, message, channelId, cancellationToken: Context.ConnectionAborted);

        await Clients.OthersInGroup(channelId).UserStoppedTyping(channelId, participant.Id);

        if (addedMessage is not null)
        {
            await TextGroup(channelId).ReceivedMessage(addedMessage.ToDto());
        }
    }

    [HubMethodName(HubMethods.IsTyping)]
    public async Task IsTyping(ChannelId channelId)
    {
        var userId = UserId;

        var shouldNotify = await _connectionManager.SetUserIsTypingAsync(channelId, userId);
        if (shouldNotify)
        {
            await Clients.OthersInGroup(channelId).UserIsTyping(channelId, userId);
        }
    }

    [HubMethodName(HubMethods.StoppedTyping)]
    public async Task StoppedTyping(ChannelId channelId)
    {
        await Clients.OthersInGroup(channelId).UserStoppedTyping(channelId, UserId);
    }

    private static readonly List<MessageDto> Empty = [];

    [HubMethodName(HubMethods.GetMessagePage)]
    public async Task<List<MessageDto>> GetMessagePage(
        ChannelId channelId, TextChannelCursor cursor, MessageRepository messageRepository)
    {
        var participant = await GetChannelParticipantAsync(channelId);
        if (participant is null)
        {
            return Empty;
        }

        var messages = await messageRepository.GetPageAsync(channelId, cursor, Context.ConnectionAborted);
        return messages.Select(x => x.ToDto()).ToList();
    }

    [HubMethodName(HubMethods.CreateChannel)]
    public async Task<ChannelId?> CreateChannel(
        List<UserId> participantIds, ChannelRepository channelRepository, SystemMessageService systemMessageService)
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

        var channel = await channelRepository.CreateChannelAsync(participants, Context.ConnectionAborted);
        if (channel is null)
        {
            return null;
        }

        await NotifyNewChannelParticipantsAsync(channel);

        // Don't send the system message for dms
        // But send for solo and more groups
        if (participants.Count != 2)
        {
            await systemMessageService.SendChannelCreatedMessageAsync(creatingUser, channel.Id);
        }

        return channel.Id;
    }

    private async Task NotifyNewChannelParticipantsAsync(Channel channel)
    {
        foreach (var user in channel.Participants)
        {
            var userConnections = await _connectionManager.GetUserConnectionsAsync(user);
            foreach (var connection in userConnections)
            {
                await Groups.AddToGroupAsync(connection, channel.Id);
            }
        }

        await Clients.Groups(channel.Id).AddedToChannel(channel.ToDto());
    }
}
