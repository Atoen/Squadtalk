using Microsoft.AspNetCore.SignalR;
using Shared;
using Shared.Data.TypedIds;
using Shared.Enums;
using Squadtalk.Data;
using Squadtalk.Data.Entities;
using Squadtalk.Extensions;
using Squadtalk.Hubs;
using tusdotnet.Interfaces;

namespace Squadtalk.Services;

public class SystemMessageService
{
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;
    private readonly MessageStorageService _messageStorageService;
    private readonly FileStorageService _fileStorageService;
    private readonly EmbedService _embedService;

    public SystemMessageService(
        IHubContext<ChatHub, IChatClient> hubContext,
        MessageStorageService messageStorageService,
        FileStorageService fileStorageService,
        EmbedService embedService)
    {
        _hubContext = hubContext;
        _messageStorageService = messageStorageService;
        _fileStorageService = fileStorageService;
        _embedService = embedService;
    }

    public Task SendChannelCreatedMessageAsync(ApplicationUser user, ChannelId channelId)
    {
        var content = $"{user.UserName} has created this channel.";
        return SendSystemMessageAsync(user, channelId, SystemMessageType.ChannelCreated, content);
    }

    public Task SendChannelNameChangedMessageAsync(ApplicationUser user, ChannelId channelId, string newName)
    {
        var content = $"{user.UserName} has changed the channel name to \"{newName}\".";
        return SendSystemMessageAsync(user, channelId, SystemMessageType.ChannelNameChanged, content);
    }

    public Task SendCallStartedMessageAsync(ApplicationUser user, ChannelId channelId, string callId)
    {
        var content = $"{user.UserName} has started a call.";
        return SendSystemMessageAsync(user, channelId, SystemMessageType.CallStarted, content);
    }

    public Task SendCallEndedMessageAsync(ApplicationUser user, ChannelId channelId, TimeSpan callDuration, bool callMissed, string callId)
    {
        var content = $@"{user.UserName} has started a call that lasted {callDuration:hh\:mm\:ss}.";
        var messageType = SystemMessageType.CallEnded;

        return SendSystemMessageAsync(user, channelId, messageType, content);
    }

    public async Task SendSystemMessageAsync(ApplicationUser user, ChannelId channelId, SystemMessageType messageType, string content)
    {
        var embed = new Embed
        {
            Type = EmbedType.SystemMessage,
            [EmbedData.SystemMessageData] = content,
            [EmbedData.SystemMessageType] = SystemMessageTypeHelper.Format(messageType)
        };

        var message = _messageStorageService.CreateMessage(user, string.Empty, channelId)
            .WithEmbed(embed);

        await _messageStorageService.StoreMessageAsync(message);
        await _hubContext.Clients.Group(channelId).ReceiveMessage(message.ToDto());
    }

    public async Task SendFileEmbedMessageAsync(ApplicationUser user, ITusFile file, CancellationToken cancellationToken)
    {
        var metadata = await file.GetMetadataAsync(cancellationToken);
        var channelId = (ChannelId) metadata.GetString(EmbedData.ChannelId);

        await _fileStorageService.StoreFileAsync(file, channelId);

        var embed = await _embedService.CreateFileEmbedAsync(file, channelId, cancellationToken);

        var message = _messageStorageService.CreateMessage(user, string.Empty, channelId)
            .WithEmbed(embed);

        await _messageStorageService.StoreMessageAsync(message);

        await _hubContext.Clients.Group(channelId).ReceiveMessage(message.ToDto());
    }
}
