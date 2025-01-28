using Shared.Data.TypedIds;
using Shared.Enums;
using Shared.Extensions;
using Shared.Reactive;

namespace Shared.Models;

public sealed class GroupChatModel : ChatModel, ISubscriber<UserModel>, IDisposable
{
    public const string GlobalChanelIdValue = "global";

    public static readonly GroupId GlobalChatId = new(GlobalChanelIdValue);
    public static GroupChatModel CreateGlobalChat() => new([], GlobalChatId) { _name = "Global" };

    public override List<GroupParticipantModel> Participants { get; }
    public override List<GroupParticipantModel> Others { get; }

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

    public GroupChatModel(IEnumerable<GroupParticipantModel> participants, GroupId id, string? customName = null) : base(id)
    {
        _customName = customName;

        Participants = participants.OrderBy(x => x.Username()).ToList();
        Others = Participants.Where(x => x.User.IsRemote).ToList();

        Subscribe(Others);
    }

    private readonly List<IDisposable?> _subscriptions = [];

    private void Subscribe(List<GroupParticipantModel> participants)
    {
        foreach (var participant in participants)
        {
            var subscription = participant.User.Subscribe(this);
            if (subscription is not null)
            {
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
        if (_subscriptions is not { Count: > 0 })
        {
            return;
        }

        foreach (var subscription in _subscriptions)
        {
            subscription?.Dispose();
        }
    }

    public override void UpdateParticipants(IEnumerable<GroupParticipantModel> updatedParticipants)
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
            : string.Join(", ", Others.Take(3).Select(x => x.User.Username));

        return _name;
    }

    private UserStatus GetStatus()
    {
        var hasOnlineUser = Others.Any(x => x.User.Status == UserStatus.Online);
        return hasOnlineUser ? UserStatus.Online : UserStatus.Offline;
    }
}
