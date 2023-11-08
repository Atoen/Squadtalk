using Squadtalk.Client.Models;
using Squadtalk.Shared;

namespace Squadtalk.Client.Services;

public delegate void UserListChangedDelegate();
public class UserService
{
    private readonly JwtService _jwtService;
    private readonly SignalRService _signalRService;
    public List<UserModel> Users { get; } = new();

    public event UserListChangedDelegate? UserListChanged;

    public UserService(JwtService jwtService, SignalRService signalRService)
    {
        _jwtService = jwtService;
        _signalRService = signalRService;
        _signalRService.ConnectedUsersReceived += SignalRServiceOnConnectedUsersReceived;
        _signalRService.UserDisconnected += SignalRServiceOnUserDisconnected;
        _signalRService.UserConnected += SignalRServiceOnUserConnected;
    }

    private void SignalRServiceOnUserConnected(UserDto user)
    {
        if (user.Username == _jwtService.Username) return;
        
        Users.Add(new UserModel
        {
            Username = user.Username,
            Status = UserStatus.Online,
            AvatarUrl = "user.png",
            Color = "whitesmoke",
            Id = user.Id
        });
        
        UserListChanged?.Invoke();
    }

    private void SignalRServiceOnConnectedUsersReceived(IEnumerable<UserDto> users)
    {
        Users.Clear();
        
        foreach (var user in users)
        {
            Users.Add(new UserModel
            {
                Username = user.Username,
                Status = UserStatus.Online,
                AvatarUrl = "user.png",
                Color = "whitesmoke",
                Id = user.Id
            });
        }
        
        UserListChanged?.Invoke();
    }
    
    private void SignalRServiceOnUserDisconnected(UserDto user)
    {
        if (user.Username == _jwtService.Username) return;
        
        Users.RemoveAll(x => x.Username == user.Username);
        UserListChanged?.Invoke();
    }
}