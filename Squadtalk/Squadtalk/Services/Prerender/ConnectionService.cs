using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Shared.Services;
using Squadtalk.Data;
using Squadtalk.Data.Entities;
using Squadtalk.Data.Repositories;

namespace Squadtalk.Services.Prerender;

internal class ConnectionService : IConnectionService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly ChatUserRepository _chatUserRepository;
    private readonly FriendRepository _friendRepository;
    private readonly MessageRepository _messageRepository;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly PrerenderPersistantState _persistState;

    event Action<ConnectionStatus>? IConnectionService.ConnectionStatusChanged { add { } remove { } }

    public ConnectionStatus ConnectionStatus => ConnectionStatus.Connecting;

    private Task? _connectTask;

    public ConnectionService(
        AuthenticationStateProvider authenticationStateProvider,
        ChatUserRepository chatUserRepository,
        FriendRepository friendRepository,
        MessageRepository messageRepository,
        SignInManager<ApplicationUser> signInManager,
        PrerenderPersistantState persistState)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _chatUserRepository = chatUserRepository;
        _friendRepository = friendRepository;
        _messageRepository = messageRepository;
        _signInManager = signInManager;
        _persistState = persistState;
    }

    public Task<TimeSpan> MeasureConnectionDelayAsync() => Task.FromResult<TimeSpan>(default);

    public Task OnPersisting()
    {
        _persistState.PersistData();

        return Task.CompletedTask;
    }

    public async Task ConnectAsync()
    {
        _connectTask ??= ConnectInternalAsync();
        await _connectTask;
    }

    private async Task ConnectInternalAsync()
    {
        var authenticationState = await _authenticationStateProvider.GetAuthenticationStateAsync();

        var user = await _chatUserRepository.FindUserByIdAsync(authenticationState.User, GroupInclusionOption.IncludeWithParticipants);
        if (user is null)
        {
            await _signInManager.SignOutAsync();
            return;
        }

        var friends = await _friendRepository.GetUserFriendsAsync(user.Id);
        var friendRequests = await _friendRepository.GetUserPendingFriendRequests(user.Id);

        var unreadPerGroup = await _messageRepository.GetUnreadMessageCountPerGroupAsync(user.Id);

        var groupDtos = user.Groups
            .Select(x => x.ToDto())
            .OrderByDescending(x => x.LastMessage?.Timestamp)
            .ToList();

        foreach (var groupDto in groupDtos)
        {
            groupDto.MessagesSince = unreadPerGroup.GetValueOrDefault(groupDto.Id);
        }

        var friendDtos = friends
            .Select(x => x.ToDto())
            .ToList();

        var friendRequestDtos = friendRequests
            .Select(x => x.ToDto())
            .ToList();

        _persistState.AddData(groupDtos, friendDtos, friendRequestDtos);
    }
}
