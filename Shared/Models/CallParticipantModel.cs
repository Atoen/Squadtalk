using Shared.Data;
using Shared.Data.TypedIds;
using Shared.Enums;
using Shared.Reactive;
using Shared.Services;

namespace Shared.Models;

public sealed class CallParticipantModel
    : Observable<CallParticipantModel>,
      ISubscriber,
      IDisposable, IChatUser,
      IKeyId<UserId>
{
    private bool _isSpeaking;
    private bool _microphoneEnabled;
    private Volume _volume;
    private ConnectionQuality _connectionQuality;

    public UserModel User { get; }

    public bool IsSpeaking
    {
        get => _isSpeaking;
        set => SetField(ref _isSpeaking, value);
    }

    public bool MicrophoneEnabled
    {
        get => _microphoneEnabled;
        set => SetField(ref _microphoneEnabled, value);
    }

    public Volume Volume
    {
        get => _volume;
        set => SetField(ref _volume, value);
    }

    public ConnectionQuality ConnectionQuality
    {
        get => _connectionQuality;
        set => SetField(ref _connectionQuality, value);
    }

    public bool IsRemote => User.IsRemote;
    public string Username => User.Username;
    public UserId Id => User.Id;
    public UserStatus Status => User.Status;

    public UserId Key => User.Id;

    private readonly IDisposable? _userSubscription;

    public CallParticipantModel(UserModel user)
    {
        User = user;
        _userSubscription = user.Subscribe(this);
    }

    public void OnChange() => Notify();

    public void Dispose() => _userSubscription?.Dispose();

}

