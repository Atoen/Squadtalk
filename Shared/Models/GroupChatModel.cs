using Shared.Data.TypedIds;
using Shared.Enums;
using Shared.Extensions;
using Shared.Reactive;

namespace Shared.Models;

public sealed class GroupChatModel : ChatModel, ISubscriber<UserModel>, IDisposable
{
    public override List<GroupParticipantModel> Participants { get; }
    public override List<GroupParticipantModel> Others { get; }
    public override GroupParticipantModel LocalUser { get; }

    private string? _defaultName;
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
        LocalUser = Participants.Single(x => x.User.IsLocal);

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

    public override void UpdateParticipantRole(UserId userId, GroupRole groupRole)
    {
        var existingParticipant = Participants.FirstOrDefault(x => x.Id() == userId);
        if (existingParticipant is not null)
        {
            existingParticipant.Role = groupRole;
            Notify(this);
        }
    }

    public override void UpdateParticipants(IEnumerable<GroupParticipantModel> updatedParticipants)
    {
        var updated = updatedParticipants.ToList();

        Participants.Clear();
        Participants.AddRange(updated);

        Others.Clear();
        Others.AddRange(updated.Where(x => x.IsRemote()));

        _defaultName = null;

        Notify(this);
    }

    private string GetOrPrepareName()
    {
        if (_defaultName is not null)
        {
            return _defaultName;
        }

        _defaultName = Others.Count == 0
            ? string.Empty // Group with only 1 user has localized default name
            : string.Join(", ", Others.Take(3).Select(x => x.User.Username));

        return _defaultName;
    }

    private UserStatus GetStatus()
    {
        var hasOnlineUser = Others.Any(x => x.User.Status == UserStatus.Online);
        return hasOnlineUser ? UserStatus.Online : UserStatus.Offline;
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
}
