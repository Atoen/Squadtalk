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

    public const string SendMessage = nameof(SendMessage);
    public const string IsTyping = nameof(IsTyping);
    public const string StoppedTyping = nameof(StoppedTyping);
    public const string GetMessagePage = nameof(GetMessagePage);
    public const string CreateChannel = nameof(CreateChannel);
    public const string AddFriendsToChannel = nameof(AddFriendsToChannel);
    public const string ChangeChannelName = nameof(ChangeChannelName);
}
