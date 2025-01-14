using MudBlazor;
using Shared.Extensions;
using Shared.Models;
using Squadtalk.Client.Localization;

namespace Squadtalk.Client.Services;

internal class NotificationService
{
    private readonly ISnackbar _snackbarHost;
    private readonly LocalizedText _localizedText;

    public event Func<IncomingFriendRequest, Task>? FriendRequestAcceptedFromNotification;

    public NotificationService(ISnackbar snackbarHost, LocalizedText localizedText)
    {
        _snackbarHost = snackbarHost;
        _localizedText = localizedText;
    }

    public void IncomingFriendRequest(IncomingFriendRequest incomingFriendRequest)
    {
        var message = _localizedText.R.friend_request_from.Format(incomingFriendRequest.From.Username);

        _snackbarHost.Add(message, Severity.Info, options =>
        {
            options.Action = _localizedText.R.friend_request_accept;
            options.ActionColor = Color.Success;
            options.Icon = Icons.Material.Rounded.EmojiPeople;
            options.Onclick = snackbar => FriendRequestAcceptedFromSnackbar(snackbar, incomingFriendRequest);
        });
    }

    public void UserAcceptedFriendRequest(OutgoingFriendRequest outgoingFriendRequest)
    {
        var message = _localizedText.R.user_accepted_friend_request.Format(outgoingFriendRequest.To.Username);
        _snackbarHost.Add(message, Severity.Info);
    }

    public void FailedToRespondToFriendRequest(IncomingFriendRequest incomingFriendRequest)
    {
        var message = _localizedText.R.failed_to_respond_to_friend_request.Format(incomingFriendRequest.From.Username);
        _snackbarHost.Add(message, Severity.Error);
    }

    public void FailedToSendFriendRequest(string requestRecipient)
    {
        var message = _localizedText.R.failed_to_send_friend_request_to.Format(requestRecipient);
        _snackbarHost.Add(message, Severity.Error);
    }

    public void FailedToCancelFriendRequest(OutgoingFriendRequest outgoingFriendRequest)
    {
        var message = _localizedText.R.failed_to_cancel_friend_request_from.Format(outgoingFriendRequest.To.Username);
        _snackbarHost.Add(message, Severity.Error);
    }

    public void FailedToRemoveFriend(UserModel user)
    {
        var message = _localizedText.R.failed_to_remove_friend.Format(user.Username);
        _snackbarHost.Add(message, Severity.Error);
    }

    public void UnableToConnect()
    {
        _snackbarHost.Add(_localizedText.R.unable_to_connect_to_the_server, Severity.Error);
    }

    public void FailedToConnectToChatHub()
    {

    }

    public void LostConnectionToServer()
    {

    }

    public void AttemptingToReconnect()
    {

    }

    public void Reconnected()
    {

    }

    private Task FriendRequestAcceptedFromSnackbar(Snackbar snackbar, IncomingFriendRequest incomingFriendRequest)
    {
        _snackbarHost.Remove(snackbar);

        return FriendRequestAcceptedFromNotification.TryInvoke(incomingFriendRequest);
    }
}
