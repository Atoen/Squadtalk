namespace Shared.Signalr;

public static class HubMethods
{
    public const string SendFriendRequest = nameof(SendFriendRequest);
    public const string CancelFriendRequest = nameof(CancelFriendRequest);
    public const string RespondToFriendRequest = nameof(RespondToFriendRequest);
    public const string RemoveFriend = nameof(RemoveFriend);
    public const string GetFriendList = nameof(GetFriendList);
    public const string GetFriendRequests = nameof(GetFriendRequests);

    public const string ChangeStatus = nameof(ChangeStatus);
    public const string GetSelfStatus = nameof(GetSelfStatus);
}
