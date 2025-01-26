using Shared.Data.TypedIds;
using Shared.Enums;
using Shared.Reactive;

namespace Shared.Models;

public class GroupChatModel : ChannelModel, ISubscriber<UserModel>, IDisposable
{
    public const string GlobalChanelIdValue = "global";

    public static readonly ChannelId GlobalChatId = new(GlobalChanelIdValue);
    public static GroupChatModel CreateGlobalChat() => new([], GlobalChatId) { _name = "Global" };
    
    public override List<UserModel> Others { get; }

    private string? _name;
    public override string Name => CustomName ?? GetOrPrepareName();

    private string? _customName;

    public string? CustomName
    {
        get => _customName;
        set => SetField(ref _customName, value);
    }

    private UserStatus _previousStatus;
    public override UserStatus Status => GetStatus();

    public bool HasOnlineStatus => Status == UserStatus.Online;

    public GroupChatModel(IEnumerable<UserModel> others, ChannelId id, string? customName = null) : base(id)
    {
        _customName = customName;

        var otherUsers = others.ToList();
        Subscribe(otherUsers);
        Others = otherUsers;
    }

    private List<IDisposable?>? _subscriptions;

    private void Subscribe(List<UserModel> users)
    {
        foreach (var user in users)
        {
            var subscription = user.Subscribe(this);
            if (subscription is not null)
            {
                _subscriptions ??= [];
                _subscriptions.Add(subscription);
            }
        }
    }

    // Propagating notification about status change
    public void OnNext(UserModel value)
    {
        var status = GetStatus();
        if (status != _previousStatus)
        {
            _previousStatus = status;
            Notify(this);
        }
    }

    public void Dispose()
    {
        if (_subscriptions is not { Count: > 0 } subscriptions)
        {
            return;
        }

        foreach (var subscription in subscriptions)
        {
            subscription?.Dispose();
        }
    }

    public override void UpdateParticipants(IEnumerable<UserModel> updatedParticipants)
    {
        Others.Clear();
        Others.AddRange(updatedParticipants);

        _name = null;

        Notify(this);
    }

    private string GetOrPrepareName()
    {
        if (_name is not null)
        {
            return _name;
        }

        _name = Others.Count == 0
            ? string.Empty // Group with only 1 user has localized default name
            : string.Join(", ", Others.Take(3).Select(x => x.Username));

        return _name;
    }

    private UserStatus GetStatus()
    {
        var hasOnlineUser = Others.Any(x => x.Status == UserStatus.Online);
        return hasOnlineUser ? UserStatus.Online : UserStatus.Offline;
    }
}
