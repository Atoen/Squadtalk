using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Shared.Communication;
using Shared.Data;
using Shared.Enums;
using Shared.Extensions;
using Shared.Models;
using Shared.Services;
using Squadtalk.Client.Extensions;

namespace Squadtalk.Client.Services;

public class TextChatService : ITextChatService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly ICreateTextChannelRequestHandler _createTextChannelRequestHandler;
    private readonly ICommunicationService _communicationService;
    private readonly ILogger<TextChatService> _logger;
    private readonly NavigationManager _navigationManager;

    private readonly List<TextChannelModel> _allChannels = [];
    private readonly List<GroupChatModel> _groupChats = [];
    private readonly List<DirectMessageChannelModel> _directMessageChannels = [];

    public IReadOnlyList<UserModel> Users => UserModel.Models;
    public IReadOnlyList<TextChannelModel> AllChannels => _allChannels;
    public IReadOnlyList<GroupChatModel> GroupChats => _groupChats;
    public IReadOnlyList<DirectMessageChannelModel> DirectMessageChannels => _directMessageChannels;

    private UserId? _userId;

    public TextChatService(
        AuthenticationStateProvider authenticationStateProvider,
        ICreateTextChannelRequestHandler createTextChannelRequestHandler,
        ICommunicationService communicationService,
        ILogger<TextChatService> logger,
        NavigationManager navigationManager)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _createTextChannelRequestHandler = createTextChannelRequestHandler;
        _communicationService = communicationService;
        _navigationManager = navigationManager;
        _logger = logger;

        _communicationService.UserConnected += user => UserConnected(user, false);
        _communicationService.UserDisconnected += UserDisconnected;
        _communicationService.ConnectedUsersReceived += ReceivedConnectedUsers;
        _communicationService.TextChannelsReceived += TextChannelsReceived;
        _communicationService.AddedToTextChannel += AddedToTextChannel;
    }

    public event Action? StateChanged;
    public event Func<Task>? StateChangedAsync;
    public event Action? ChannelChanged;
    public event Func<Task>? ChannelChangedAsync;

    public TextChannelModel? CurrentChannel { get; private set; }

    public TextChannelModel? GetChannel(ChannelId id)
    {
        return id == GroupChatModel.GlobalChatId
            ? GroupChatModel.GlobalChat
            : AllChannels.FirstOrDefault(x => x.Id == id);
    }

    public async Task OpenChannelAsync(TextChannelModel channelModel)
    {
        await ChangeChannelAsync(channelModel);
        _navigationManager.NavigateTo("Messages/Chat");
    }

    public Task ClearChannelSelectionAsync() => ChangeChannelAsync(null);

    public async Task OpenOrCreateFakeDirectMessageChannel(UserModel model)
    {
        var authenticationState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var id = authenticationState.User.GetRequiredClaimValue(ClaimTypes.NameIdentifier);

        if (id == model.Id) return;

        var openDirectMessageChannelWithUser = DirectMessageChannels.FirstOrDefault(x => x.Other.Id == model.Id);
        if (openDirectMessageChannelWithUser is not null)
        {
            await OpenChannelAsync(openDirectMessageChannelWithUser);
            return;
        }

        await OpenChannelAsync(DirectMessageChannelModel.CreateFakeChannel(model));
    }

    public async Task CreateRealDirectMessageChannel(TextChannelModel channelModel)
    {
        _userId ??= await GetUserIdAsync();

        var otherUserId = ((DirectMessageChannelModel) channelModel).Other.Id;
        var participants = new List<UserId> { _userId, otherUserId };

        var channelId = await OpenNewChannel(participants);

        if (channelId is not null && GetChannel(channelId) is { } openedChannel)
        {
            await ChangeChannelAsync(openedChannel);
        }
    }

    private Task ChangeChannelAsync(TextChannelModel? channel)
    {
        if (CurrentChannel == channel)
        {
            return Task.CompletedTask;
        }

        CurrentChannel = channel;
        if (CurrentChannel is not null)
        {
            CurrentChannel.State.UnreadMessages = 0;
        }

        ChannelChanged?.Invoke();
        return ChannelChangedAsync.TryInvoke();
    }

    private Task<ChannelId?> OpenNewChannel(List<UserId> participants)
    {
        return _createTextChannelRequestHandler.CreateTextChannelAsync(participants);
    }

    private async Task AddedToTextChannel(IChatChannel channel)
    {
        await AddChannel(channel, false);
        await StateChangedAsync.TryInvoke();
    }

    private async Task TextChannelsReceived(IEnumerable<IChatChannel> channels)
    {
        foreach (var channel in channels)
        {
            await AddChannel(channel, true);
        }

        StateChanged?.Invoke();
        await StateChangedAsync.TryInvoke();
    }

    private async Task AddChannel(IChatChannel channel, bool bulk)
    {
        if (_allChannels.Exists(x => x.Id == channel.Id)) return;
        _userId ??= await GetUserIdAsync();

        var model = CreateChannelModel(channel, _userId);
        if (!bulk)
        {
            model.State.ReachedEnd = true;
        }

        _allChannels.Add(model);

        if (model is GroupChatModel groupChat)
        {
            _groupChats.Add(groupChat);
        }
        else
        {
            var directMessageChannel = (DirectMessageChannelModel) model;
            _directMessageChannels.Add(directMessageChannel);

            await CheckIfNeedToUpgradeCurrentFakeChannelToReal(directMessageChannel);
        }

        if (!bulk)
        {
            StateChanged?.Invoke();
            await StateChangedAsync.TryInvoke();
        }
    }

    private Task CheckIfNeedToUpgradeCurrentFakeChannelToReal(DirectMessageChannelModel openedDirectMessageChannelModel)
    {
        if (CurrentChannel is DirectMessageChannelModel { Id.Value: DirectMessageChannelModel.FakeChannelIdValue } currentFakeDm &&
            currentFakeDm.Other.Id == openedDirectMessageChannelModel.Other.Id)
        {
            return ChangeChannelAsync(openedDirectMessageChannelModel);
        }

        return Task.CompletedTask;
    }

    private TextChannelModel CreateChannelModel(IChatChannel channel, UserId id)
    {
        var lastMessageIsByCurrentUser = channel.LastMessage?.Author.Id == id;

        _logger.LogInformation("Creating model");

        var othersInChannel = channel.Participants.Where(x => x.Id != id).ToList();

        TextChannelModel model = othersInChannel switch
        {
            [var other] => new DirectMessageChannelModel(UserModel.GetOrCreate(other), channel.Id),
            { Count: > 1 } => new GroupChatModel(othersInChannel.Select(UserModel.GetOrCreate), channel.Id),
            _ => throw new InvalidOperationException()
        };

        return model.WithLastMessage(channel.LastMessage, lastMessageIsByCurrentUser);
    }

    private async Task ReceivedConnectedUsers(IEnumerable<IChatUser> users)
    {
        foreach (var user in users)
        {
            _logger.LogInformation("User {@User} connected in bulk", user.Username);
            await UserConnected(user, true);
        }

        StateChanged?.Invoke();
        await StateChangedAsync.TryInvoke();
    }

    private async Task UserConnected(IChatUser user, bool bulkAdd)
    {
        _userId ??= await GetUserIdAsync();

        if (user.Id == _userId) return;

        var model = UserModel.GetOrCreate(user);
        model.Status = UserStatus.Online;

        if (!bulkAdd)
        {
            StateChanged?.Invoke();
            await StateChangedAsync.TryInvoke();
        }
    }

    private async Task UserDisconnected(IChatUser user)
    {
        _userId ??= await GetUserIdAsync();

        if (user.Id == _userId) return;

        var openDirectMessageChannelWithUser = DirectMessageChannels.FirstOrDefault(x => x.Other.Id == user.Id);
        if (openDirectMessageChannelWithUser is null)
        {
            UserModel.Models.RemoveAll(x => x.Id == user.Id);
        }
        else
        {
            UserModel.Models.First(x => x.Id == user.Id).Status = UserStatus.Offline;
        }

        StateChanged?.Invoke();
        await StateChangedAsync.TryInvoke();
    }

    private async ValueTask<UserId> GetUserIdAsync()
    {
        var authenticationState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var claimValue = authenticationState.User.GetRequiredClaimValue(ClaimTypes.NameIdentifier);
        return new UserId(claimValue);
    }
}
