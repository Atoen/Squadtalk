using RestSharp;
using Squadtalk.Client.Models;
using Squadtalk.Client.Models.Communication;
using Squadtalk.Client.Shared;
using Squadtalk.Shared;

namespace Squadtalk.Client.Services;

public delegate void StateChangedHandler();

public delegate void ChannelChangedHandler();

public class CommunicationManager
{
    private readonly JwtService _jwtService;
    private readonly RestClient _restClient;

    private Channel _currentChannel = GroupChat.GlobalChat;

    public CommunicationManager(JwtService jwtService, RestClient restClient)
    {
        _jwtService = jwtService;
        _restClient = restClient;

        SignalRService.ConnectedUsersReceived += ReceivedConnectedUsers;
        SignalRService.UserDisconnected += UserDisconnected;
        SignalRService.UserConnected += UserConnected;
        SignalRService.ChannelsReceived += AddChannels;
        SignalRService.AddedToChannel += AddChannel;
    }

    public List<User> Users { get; } = new();
    
    public List<Channel> AllChannels { get; } = new(); 
    public List<GroupChat> GroupChats { get; } = new();
    public List<DirectMessageChannel> DirectMessageChannels { get; } = new();

    public Channel CurrentChannel
    {
        get => _currentChannel;
        private set
        {
            var last = _currentChannel;
            _currentChannel = value;
            _currentChannel.IsSelected = true;

            if (_currentChannel != last)
            {
                ChannelChanged?.Invoke();
            }
        }
    }

    public ChannelState CurrentChannelState => _currentChannel.State;

    public event StateChangedHandler? StateChanged;
    public event ChannelChangedHandler? ChannelChanged;

    public Channel? GetChannel(Guid id)
    {
        return id == GroupChat.GlobalChatId
            ? GroupChat.GlobalChat
            : AllChannels.FirstOrDefault(x => x.Id == id);
    }

    private void AddChannels(IEnumerable<ChannelDto> channels)
    {
        Console.WriteLine("Channels received");
        foreach (var channel in channels)
        {
            AddChannel(channel);
        }
    }

    private void AddChannel(ChannelDto channel) => AddChannel(channel, false);

    private void AddChannel(ChannelDto channelDto, bool select)
    {
        if (AllChannels.Exists(x => x.Id == channelDto.Id)) return;

        if (CurrentChannel.IsFake())
        {
            select = true;
        }
        
        Console.WriteLine(channelDto);
        Console.WriteLine(string.Join(", ", channelDto.Participants.Select(x => x.Username)));
        var model = CreateChannelModel(channelDto);
        
        AllChannels.Add(model);

        if (model is GroupChat groupChat)
        {
            GroupChats.Add(groupChat);
        }
        else
        {
            var directMessageChannel = (DirectMessageChannel) model;
            DirectMessageChannels.Add(directMessageChannel);
        }

        if (select)
        {
            CurrentChannel = model;
            ChannelChanged?.Invoke();
        }

        StateChanged?.Invoke();
    }

    private Channel CreateChannelModel(ChannelDto channelDto)
    {
        var others = channelDto.Participants.Where(x => x.Id != _jwtService.Id).ToList();
        if (others.Count > 1)
        {
            return new GroupChat(channelDto.Id)
            {
                Others = others
            };
        }

        var other = others[0];
        var user = Users.FirstOrDefault(x => x.Id == other.Id);
        if (user is null)
        {
            user = CreateUserModel(other, UserStatus.Offline);
            Users.Add(user);
        }

        return new DirectMessageChannel(channelDto.Id)
        {
            Other = user
        };
    }

    private User CreateUserModel(UserDto userDto, UserStatus status) => new()
    {
        Username = userDto.Username,
        Id = userDto.Id,
        Status = status,
        Color = "whitesmoke",
        AvatarUrl = "user.png"
    };

    public void OpenOrCreateFakeDirectMessageChannel(User other)
    {
        if (other.Id == _jwtService.Id) return;

        CurrentChannel.IsSelected = false;

        var openDirectMessageChannelWithUser = DirectMessageChannels.FirstOrDefault(x => x.Other.Id == other.Id);
        if (openDirectMessageChannelWithUser is not null)
        {
            CurrentChannel = openDirectMessageChannelWithUser;
            return;
        }
        
        CurrentChannel = DirectMessageChannel.CreateFakeChannel(other);
    }

    public async Task OpenNewChannel(List<Guid> participants)
    {
        var request = new RestRequest("api/message/createChannel")
            .AddHeader("Authorization", $"Bearer ${_jwtService.Token}")
            .AddBody(participants);

        var channelDto =  await _restClient.PostAsync<ChannelDto>(request);
        AddChannel(channelDto!, true);
    }

    public void ChangeChannel(Guid id)
    {
        CurrentChannel.IsSelected = false;

        if (id == GroupChat.GlobalChatId)
        {
            CurrentChannel = GroupChat.GlobalChat;
            return;
        }

        var newChannel = GetChannel(id);
        if (newChannel is null)
        {
            Console.WriteLine("Cannot find selected channel");
            return;
        }

        newChannel.IsSelected = true;
    }

    public void ReceivedConnectedUsers(IEnumerable<UserDto> users)
    {
        Users.Clear();

        foreach (var user in users)
        {
            UserConnected(user, true);
        }
        
        StateChanged?.Invoke();
    }

    private void UserConnected(UserDto user)
    {
        UserConnected(user, false);
    }

    public void UserConnected(UserDto userDto, bool bulkAdd)
    {
        if (!bulkAdd && userDto.Id == _jwtService.Id) return;

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

    public void UserDisconnected(UserDto userDto)
    {
        if (userDto.Id == _jwtService.Id) return;

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