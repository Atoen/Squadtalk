﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Squadtalk.Server.Models;
using Squadtalk.Server.Services;
using Squadtalk.Shared;

namespace Squadtalk.Server.Hubs;

[Authorize]
public class ChatHub : Hub<IChatClient>
{
    private readonly ChannelService _channelService;
    private readonly ConnectionManager _connectionManager;
    private readonly MessageService _messageService;
    private readonly UserService _userService;

    public ChatHub(UserService userService, MessageService messageService, ConnectionManager connectionManager,
        ChannelService channelService)
    {
        _userService = userService;
        _messageService = messageService;
        _connectionManager = connectionManager;
        _channelService = channelService;
    }

    public async Task SendMessage(string messageContent, Guid channelId, IGifSourceVerifier gifSourceVerifier)
    {
        var user = await _userService.GetUserAsync(Context.User!);

        var isGifSource = await gifSourceVerifier.VerifyAsync(messageContent);

        var message = new Message
        {
            Author = user.AsT0,
            Timestamp = DateTimeOffset.Now,
            Content = isGifSource ? string.Empty : messageContent,
            ChannelId = channelId
        };

        if (isGifSource)
        {
            message.Embed = new Embed
            {
                Type = EmbedType.Gif,
                Data = new Dictionary<string, string>
                {
                    { "Uri", messageContent }
                }
            };
        }

        await _messageService.StoreMessageAsync(message);
        await Clients.Group(channelId.ToString()).ReceiveMessage(message.ToDto());
    }

    public override async Task OnConnectedAsync()
    {
        var result = await _userService.GetUserAsync(Context.User!);
        var user = result.AsT0;
        var userDto = user.ToDto();

        var isUniqueUserConnection = await _connectionManager.UserConnected(userDto, Context.ConnectionId);

        await Groups.AddToGroupAsync(Context.ConnectionId, ChannelService.GlobalChannelIdString);
        if (isUniqueUserConnection)
        {
            await Clients.Group(ChannelService.GlobalChannelIdString).UserConnected(userDto);
        }

        await Clients.Caller.GetConnectedUsers(_connectionManager.ConnectedUsers);

        var channels = await _channelService.GetUserChannelsAsync(user.Username);
        if (channels is null) return;

        await AddToPrivateChannelsAsync(userDto, channels, isUniqueUserConnection);

        var channelDtos = channels.Select(x => x.ToDto);
        await Clients.Caller.GetChannels(channelDtos);
    }

    private async Task AddToPrivateChannelsAsync(UserDto user, List<Channel> channels, bool isUniqueUserConnection)
    {
        foreach (var channel in channels)
        {
            var channelId = channel.Id.ToString();

            await Groups.AddToGroupAsync(Context.ConnectionId, channelId);
            if (isUniqueUserConnection)
            {
                await Clients.Group(channelId).UserConnected(user);
            }
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var result = await _userService.GetUserAsync(Context.User!);
        var user = result.AsT0;
        var dto = user.ToDto();

        var allConnectionsClosed = await _connectionManager.UserDisconnected(dto, Context.ConnectionId);
        if (!allConnectionsClosed) return;

        await Clients.Group(ChannelService.GlobalChannelIdString).UserDisconnected(dto);
        var channels = await _channelService.GetUserChannelsAsync(dto.Username);
        if (channels is null) return;

        foreach (var channel in channels)
        {
            await Clients.Group(channel.Id.ToString()).UserDisconnected(dto);
        }
    }
}