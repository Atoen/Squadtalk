using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Enums;
using Shared.Reactive;

namespace Shared.Models;

public class UserModel : Observable<UserModel>, IEquatable<UserModel?>, IChatUser, IKeyId<UserId>
{
    private UserStatus _status;
    private string _avatarUrl = default!;
    private string _username = default!;

    public string Username
    {
        get => _username;
        set => SetField(ref _username, value);
    }

    public string AvatarUrl
    {
        get => _avatarUrl;
        set => SetField(ref _avatarUrl, value);
    }

    public UserStatus Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }

    public UserId Id { get; init; }
    
    public bool IsLocal { get; init; }
    public bool IsRemote => !IsLocal;

    public static UserModel Create(IChatUser chatUser, UserId localUserId)
    {
        return new UserModel
        {
            Id = chatUser.Id,
            IsLocal = chatUser.Id == localUserId,
            _username = chatUser.Username,
            _status = chatUser.Status,
            _avatarUrl = "user.png"
        };
    }
    public UserId Key => Id;

    public bool Equals(UserModel? other)
    {
        if (other is null) return false;

        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        if (obj is UserModel userModel)
        {
            return Equals(userModel);
        }

        return false;
    }

    public override int GetHashCode() => Id.GetHashCode();
}
