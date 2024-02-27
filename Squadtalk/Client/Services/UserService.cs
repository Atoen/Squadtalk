using Squadtalk.Shared;

namespace Squadtalk.Client.Services;

// public delegate void UserListChangedDelegate();
public class UserService
{
    private readonly CommunicationManager _communicationManager;
    private readonly JwtService _jwtService;

    private readonly SignalRService _signalRService;
    // public List<UserModel> Users { get; } = new();

    // public event UserListChangedDelegate? UserListChanged;

    public UserService(JwtService jwtService, SignalRService signalRService, CommunicationManager communicationManager)
    {
        _jwtService = jwtService;
        _signalRService = signalRService;
        _communicationManager = communicationManager;
        // _signalRService.ConnectedUsersReceived += SignalRServiceOnConnectedUsersReceived;
        // _signalRService.UserDisconnected += SignalRServiceOnUserDisconnected;
        // _signalRService.UserConnected += SignalRServiceOnUserConnected;
    }

    private void SignalRServiceOnUserConnected(UserDto user)
    {
        if (user.Username == _jwtService.Username) return;

        _communicationManager.UserConnected(user, true);

        // Users.Add(new UserModel
        // {
        //     Username = user.Username,
        //     Status = UserStatus.Online,
        //     AvatarUrl = "user.png",
        //     Color = "whitesmoke",
        //     Id = user.Id
        // });

        // UserListChanged?.Invoke();
    }

    private void SignalRServiceOnConnectedUsersReceived(IEnumerable<UserDto> users)
    {
        _communicationManager.ReceivedConnectedUsers(users);

        // Users.Clear();

        // foreach (var user in users)
        // {
        //     Users.Add(new UserModel
        //     {
        //         Username = user.Username,
        //         Status = UserStatus.Online,
        //         AvatarUrl = "user.png",
        //         Color = "whitesmoke",
        //         Id = user.Id
        //     });
        // }
        //
        // UserListChanged?.Invoke();
    }

    private void SignalRServiceOnUserDisconnected(UserDto user)
    {
        if (user.Username == _jwtService.Username) return;

        _communicationManager.UserDisconnected(user);

        // Users.RemoveAll(x => x.Username == user.Username);
        // UserListChanged?.Invoke();
    }
}