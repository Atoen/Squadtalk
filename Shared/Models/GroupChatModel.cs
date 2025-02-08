using Shared.Data.TypedIds;
using Shared.Enums;
using Shared.Extensions;
using Shared.Reactive;

namespace Shared.Models;

public sealed class GroupChatModel : ChatModel, ISubscriber, IDisposable
{
    public override List<GroupParticipantModel> Participants { get; }
    public override ObservableItemList<GroupParticipantModel> Others { get; }
    public override GroupParticipantModel LocalUser { get; }

    private string? _defaultName;
    public override string Name => CustomName ?? GetOrPrepareName();

    private string? _customName;

    public string? CustomName
    {
        get => _customName;
        set => SetField(ref _customName, value);
    }

    public override UserStatus Status => GetStatus();

    public bool HasOnlineStatus => Status == UserStatus.Online;

    private readonly IDisposable? _subscription;

    public GroupChatModel(IEnumerable<GroupParticipantModel> participants, GroupId id, string? customName = null) : base(id)
    {
        _customName = customName;

        Participants = participants.OrderBy(x => x.Username()).ToList();
        LocalUser = Participants.Single(x => x.User.IsLocal);
        
        Others = Participants.Where(x => x.User.IsRemote).ToObservableItemList();
        _subscription = Others.Subscribe(this);
    }

    public override void UpdateParticipantRole(UserId userId, GroupRole groupRole)
    {
        var existingParticipant = Participants.FirstOrDefault(x => x.Id() == userId);
        if (existingParticipant is not null)
        {
            existingParticipant.Role = groupRole;
            Notify();
        }
    }

    public override void UpdateParticipants(IEnumerable<GroupParticipantModel> updatedParticipants)
    {
        var updated = updatedParticipants.ToList();

        Participants.Clear();
        Participants.AddRange(updated);

        Others.Refresh(updated.Where(x => x.IsRemote()));
        
        var localRole = Participants.Single(x => x.IsLocal()).Role;
        LocalUser.Role = localRole;

        _defaultName = null;

        Notify();
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

    public void OnChange() => Notify();
    
    public void Dispose()
    {
        _subscription?.Dispose();
        Others.Dispose();
    }
}
