using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using RestSharp;
using Shared.Communication;
using Shared.DTOs;
using Shared.Enums;
using Shared.Models;
using Shared.Services;
using Squadtalk.Client.Extensions;

namespace Squadtalk.Client.Services;

public class CommunicationManager : ICommunicationManager
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly RestClient _restClient;
    private readonly ISignalrService _signalrService;

    private TextChannel _currentChannel = GroupChat.GlobalChat;
    
    public List<UserModel> Users { get; } = [];
    
    public List<TextChannel> AllChannels { get; } = []; 
    public List<GroupChat> GroupChats { get; } = [];
    public List<DirectMessageChannel> DirectMessageChannels { get; } = [];
    
    public CommunicationManager(AuthenticationStateProvider authenticationStateProvider, RestClient restClient, ISignalrService signalrService)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _restClient = restClient;
        _signalrService = signalrService;

        _signalrService.UserConnected += UserConnected;
        _signalrService.UserDisconnected += UserDisconnected;
        _signalrService.ConnectedUsersReceived += ReceivedConnectedUsers;
    }
    
    public event Action? StateChanged;
    public event Action? ChannelChanged;
    
    public TextChannel CurrentChannel
    {
        get => _currentChannel;
        private set
        {
            ArgumentNullException.ThrowIfNull(value);
            
            var last = _currentChannel;
            _currentChannel = value;
            _currentChannel.Selected = true;

            if (_currentChannel != last)
            {
                ChannelChanged?.Invoke();
            }
        }
    }
    
    public TextChannel? GetChannel(string id)
    {
        return id == GroupChat.GlobalChatId
            ? GroupChat.GlobalChat
            : AllChannels.FirstOrDefault(x => x.Id == id);
    }
    
    private UserModel CreateUserModel(UserDto userDto, UserStatus status) => new()
    {
        Username = userDto.Username,
        Id = userDto.Id,
        Status = status,
        Color = "black",
        AvatarUrl = "user.png"
    };

    private async Task ReceivedConnectedUsers(IEnumerable<UserDto> users)
    {
        Users.Clear();

        foreach (var user in users)
        {
            await UserConnected(user, true);
        }
        
        StateChanged?.Invoke();
    }

    private Task UserConnected(UserDto user)
    {
        return UserConnected(user, false);
    }

    private async Task UserConnected(UserDto userDto, bool bulkAdd)
    {
        var authenticationState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        if (!bulkAdd && userDto.Id == authenticationState.User.GetRequiredClaimValue(ClaimTypes.NameIdentifier)) return;

        var openDirectMessageChannelWithUser = DirectMessageChannels.FirstOrDefault(x => x.Other.Id == userDto.Id);
        if (openDirectMessageChannelWithUser is null)
        {
            Users.Add(CreateUserModel(userDto, UserStatus.Online));
        }
        else
        {
            Users.First(x => x.Id == userDto.Id).Status = UserStatus.Online;
        }

        if (!bulkAdd)
        {
            StateChanged?.Invoke();
        }
    }

    private async Task UserDisconnected(UserDto userDto)
    {
        var authenticationState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        if (userDto.Id == authenticationState.User.GetRequiredClaimValue(ClaimTypes.NameIdentifier)) return;

        var openDirectMessageChannelWithUser = DirectMessageChannels.FirstOrDefault(x => x.Other.Id == userDto.Id);
        if (openDirectMessageChannelWithUser is null)
        {
            Users.RemoveAll(x => x.Id == userDto.Id);
        }
        else
        {
            Users.First(x => x.Id == userDto.Id).Status = UserStatus.Offline;
        }

        StateChanged?.Invoke();
    }
}