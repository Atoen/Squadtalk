using Microsoft.AspNetCore.SignalR;
using Shared;
using Shared.Data.TypedIds;
using Shared.Enums;
using Squadtalk.Data;
using Squadtalk.Data.Entities;
using Squadtalk.Data.Repositories;
using Squadtalk.Extensions;
using Squadtalk.Signalr;
using tusdotnet.Interfaces;

namespace Squadtalk.Services;

public class SystemMessageService
{
    private readonly IHubContext<AppHub, IChatClient> _hubContext;
    private readonly FileRepository _fileRepository;
    private readonly MessageRepository _messageRepository;
    private readonly EmbedService _embedService;

    public SystemMessageService(
        IHubContext<AppHub, IChatClient> hubContext,
        FileRepository fileRepository,
        MessageRepository messageRepository,
        EmbedService embedService)
    {
        _hubContext = hubContext;
        _fileRepository = fileRepository;
        _messageRepository = messageRepository;
        _embedService = embedService;
    }

    // TODO: Use user ID instead of username to support updating system message on name change

    public Task SendGroupCreatedMessageAsync(ChatUser user, GroupId groupId)
    {
        var data = new Dictionary<string, string>
        {
            [EmbedData.SystemMessageDataUsername] = user.Username
        };

        return SendSystemMessageAsync(user, groupId, SystemMessageType.ChannelCreated, data);
    }

    public Task SendChannelNameChangedMessageAsync(ChatUser user, GroupId groupId, string? newName)
    {
        if (string.IsNullOrEmpty(newName))
        {
            return SendChannelNameClearedMessageAsync(user, groupId);
        }

        var data = new Dictionary<string, string>
        {
            [EmbedData.SystemMessageDataUsername] = user.Username,
            [EmbedData.SystemMessageDataChannelName] = newName
        };

        return SendSystemMessageAsync(user, groupId, SystemMessageType.ChannelNameChanged, data);
    }

    public Task SendChannelNameClearedMessageAsync(ChatUser user, GroupId groupId)
    {
        var data = new Dictionary<string, string>
        {
            [EmbedData.SystemMessageDataUsername] = user.Username
        };

        return SendSystemMessageAsync(user, groupId, SystemMessageType.ChannelNameCleared, data);
    }

    public Task SendCallStartedMessageAsync(ChatUser user, GroupId groupId, string callId)
    {
        var data = new Dictionary<string, string>
        {
            [EmbedData.SystemMessageDataUsername] = user.Username,
            [EmbedData.SystemMessageDataCallId] = callId
        };

        return SendSystemMessageAsync(user, groupId, SystemMessageType.CallStarted, data);
    }

    public Task SendCallEndedMessageAsync(ChatUser user, GroupId groupId, TimeSpan callDuration, bool callMissed, string callId)
    {
        var data = new Dictionary<string, string>
        {
            [EmbedData.SystemMessageDataUsername] = user.Username,
            [EmbedData.SystemMessageDataCallId] = callId,
            [EmbedData.SystemMessageDataDuration] = callDuration.ToString(@"hh\:mm\:ss")
        };

        return SendSystemMessageAsync(user, groupId, SystemMessageType.CallEnded, data);
    }

    public async Task SendSystemMessageAsync(ChatUser user, GroupId groupId, SystemMessageType messageType, Dictionary<string, string> data)
    {
        var embed = new Embed
        {
            Type = EmbedType.SystemMessage,
            [EmbedData.SystemMessageType] = SystemMessageTypeHelper.Format(messageType),
        };

        foreach (var (key, value) in data)
        {
            embed[key] = value;
        }

        var addedMessage = await _messageRepository.AddMessageAsync(user, string.Empty, groupId, embed);
        if (addedMessage is not null)
        {
            await _hubContext.Clients.Group(groupId).ReceivedMessage(addedMessage.ToDto());
        }
    }

    public async Task SendFileEmbedMessageAsync(ChatUser user, ITusFile file, CancellationToken cancellationToken)
    {
        var metadata = await file.GetMetadataAsync(cancellationToken);
        var channelId = (GroupId) metadata.GetString(EmbedData.ChannelId);

        await _fileRepository.AddFileAsync(file, channelId, cancellationToken);

        var embed = await _embedService.CreateFileEmbedAsync(file, channelId, cancellationToken);

        var addedMessage = await _messageRepository.AddMessageAsync(user, string.Empty, channelId, embed, cancellationToken);
        if (addedMessage is not null)
        {
            await _hubContext.Clients.Group(channelId).ReceivedMessage(addedMessage.ToDto());
        }
    }
}
