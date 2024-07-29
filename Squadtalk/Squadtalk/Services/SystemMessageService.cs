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
        var data = new Dictionary<string, string>
        {
            [EmbedData.SystemMessageDataUsername] = user.UserName!
        };

        return SendSystemMessageAsync(user, channelId, SystemMessageType.ChannelCreated, data);
    }

    public Task SendChannelNameChangedMessageAsync(ApplicationUser user, ChannelId channelId, string newName)
    {
        var data = new Dictionary<string, string>
        {
            [EmbedData.SystemMessageDataUsername] = user.UserName!,
            [EmbedData.SystemMessageDataChannelName] = newName
        };

        return SendSystemMessageAsync(user, channelId, SystemMessageType.ChannelNameChanged, data);
    }

    public Task SendChannelNameClearedMessageAsync(ApplicationUser user, ChannelId channelId)
    {
        var data = new Dictionary<string, string>
        {
            [EmbedData.SystemMessageDataUsername] = user.UserName!
        };

        return SendSystemMessageAsync(user, channelId, SystemMessageType.ChannelNameCleared, data);
    }

    public Task SendCallStartedMessageAsync(ApplicationUser user, ChannelId channelId, string callId)
    {
        var data = new Dictionary<string, string>
        {
            [EmbedData.SystemMessageDataUsername] = user.UserName!,
            [EmbedData.SystemMessageDataCallId] = callId
        };

        return SendSystemMessageAsync(user, channelId, SystemMessageType.CallStarted, data);
    }

    public Task SendCallEndedMessageAsync(ApplicationUser user, ChannelId channelId, TimeSpan callDuration, bool callMissed, string callId)
    {
        var data = new Dictionary<string, string>
        {
            [EmbedData.SystemMessageDataUsername] = user.UserName!,
            [EmbedData.SystemMessageDataCallId] = callId,
            [EmbedData.SystemMessageDataDuration] = callDuration.ToString(@"hh\:mm\:ss")
        };

        return SendSystemMessageAsync(user, channelId, SystemMessageType.CallEnded, data);
    }

    public async Task SendSystemMessageAsync(ApplicationUser user, ChannelId channelId, SystemMessageType messageType, Dictionary<string, string> data)
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
