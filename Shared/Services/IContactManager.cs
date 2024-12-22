using Shared.Data;
using Shared.Data.Results;
using Shared.DTOs.Chat;
using Shared.Models;

namespace Shared.Services;

public interface IContactManager
{
    event Action? ContactsStateChanged;
    event Action<UserModel>? ContactDisconnected;
    event Action<UserModel>? ContactConnected;

    Func<IChatUser, UserModel> UserModelProvider { get; }

    IReadOnlyCollection<UserModel> AllContacts { get; }
    IReadOnlyCollection<UserModel> FriendList { get; }
    IReadOnlyCollection<UserModel> OtherContacts { get; }

    UserModel GetOrCreateUserModel(IChatUser chatUser);

    Task<FriendRequestResult> SendFriendRequestAsync(FriendRequestDto friendRequest);

    Task AcceptFriendRequestAsync();

    Task DeclineFriendRequestAsync();

    Task RemoveFriendAsync();
}
