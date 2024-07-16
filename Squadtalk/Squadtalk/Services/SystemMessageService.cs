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

    public Task SendChannelCreatedMessageAsync(ApplicationUser user, Channel channel)
    {
        var content = $"{user.UserName} created this channel";
        return SendSystemMessageAsync(user, channel, SystemMessageType.ChannelCreated, content);
    }

    public Task SendChannelNameChangedMessageAsync(ApplicationUser user, Channel channel, string newName)
    {
        var content = $"{user.UserName} changed this channel name to '{newName}'";
        return SendSystemMessageAsync(user, channel, SystemMessageType.ChannelNameChanged, content);
    }

    public Task SendCallEndedMessageAsync(ApplicationUser user, Channel channel, TimeSpan callDuration)
    {
        var content = $"{user.UserName} initiated a call that lasted {callDuration}";
        return SendSystemMessageAsync(user, channel, SystemMessageType.CallEnded, content);
    }

    public async Task SendSystemMessageAsync(ApplicationUser user, Channel channel, SystemMessageType messageType, string content)
    {
        var embedType = messageType switch
        {
            SystemMessageType.CallEnded => EmbedType.SystemMessage,
            _ => EmbedType.InlineSystemMessage
        };

        var embed = new Embed
        {
            Type = embedType,
            [EmbedData.SystemMessageData] = content
        };

        var message = _messageStorageService.CreateMessage(user, string.Empty, channel.Id)
            .WithEmbed(embed);

        await _messageStorageService.StoreMessageAsync(message);
        await _hubContext.Clients.Group(channel.Id).ReceiveMessage(message.ToDto());
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
