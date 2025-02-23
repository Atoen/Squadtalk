using Shared.Data;
using Shared.Data.TypedIds;
using Shared.DTOs;
using Shared.Extensions;
using Shared.Models;
using Shared.Reactive;
using Shared.Services;
using Squadtalk.Client.Services.SignalR;

namespace Squadtalk.Client.Services.Rtc;

internal sealed class RtcConnectionService : IRtcConnectionService
{
    private readonly SignalrService _signalrService;
    private readonly VoiceChatInterop _voiceChatInterop;
    private readonly UserVolumeManager _volumeManager;
    private readonly IContactManager _contactManager;
    private readonly IChatManager _chatManager;
    private readonly ILogger<RtcConnectionService> _logger;

    private readonly ObservableDictionary<UserId, CallParticipantModel> _participants = [];

    public event Action<ChatModel>? IncomingCall;

    public bool ConnectedToCall => CallChat is not null;
    public ChatModel? CallChat { get; private set; }

    public CallParticipantModel LocalParticipant { get; }
    public IObservableCollection<CallParticipantModel> CallParticipants => _participants.Values;

    public RtcConnectionService(
        SignalrService signalrService,
        VoiceChatInterop voiceChatInterop,
        UserVolumeManager volumeManager,
        IContactManager contactManager,
        IChatManager chatManager,
        ILogger<RtcConnectionService> logger)
    {
        _signalrService = signalrService;
        _voiceChatInterop = voiceChatInterop;
        _volumeManager = volumeManager;
        _contactManager = contactManager;
        _chatManager = chatManager;
        _logger = logger;

        _voiceChatInterop.SetConnectionCallback(this);

        LocalParticipant = new CallParticipantModel(contactManager.LocalUserModel)
        {
            ConnectionQuality = ConnectionQuality.Unknown
        };

        _signalrService.IncomingCall += OnIncomingCall;
        _signalrService.CallEnded += OnCallEnded;
        _signalrService.CallDeclined += OnCallDeclined;
        _signalrService.CallAccepted += OnCallAccepted;
        _signalrService.CallFailed += OnCallFailed;
    }

    public async Task StartCallAsync(ChatModel chat)
    {
        var result = await _signalrService.StartVoiceCallAsync(chat.Id);
        if (result.ErrorOrValueIs(null))
        {
            _logger.LogError("Failed to start call");
            return;
        }

        await JoinRoomAsync(chat, result.Value!);
    }

    public async Task AcceptCallAsync(ChatModel chat)
    {
        var result = await _signalrService.AcceptCallAsync(chat.Id);
        if (result.ErrorOrValueIs(null))
        {
            _logger.LogError("Failed to accept the call");
            return;
        }

        await JoinRoomAsync(chat, result.Value!);
    }

    public Task DeclineCallAsync(ChatModel chat)
    {
        return _signalrService.DeclineCallAsync(chat.Id);
    }

    public async Task LeaveCallAsync()
    {
        await _voiceChatInterop.LeaveCallAsync();

        CallChat = null;
        _participants.Clear();
    }

    public CallParticipantModel? GetParticipantById(UserId userId) => _participants.GetValueOrDefault(userId);

    public async Task<bool> GroupHasActiveCallAsync(ChatModel chat)
    {
        var result = await _signalrService.GroupHasActiveCall(chat.Id);
        var hasActiveCall = result.ValueOr(false);

        chat.HasActiveCall = hasActiveCall;

        return hasActiveCall;
    }

    public async Task ChangeUserVolumeAsync(CallParticipantModel participant, Volume volume, AudioSource audioSource = AudioSource.Microphone)
    {
        participant.Volume = volume;
        _voiceChatInterop.ChangeUserVolume(participant, volume, audioSource);
        await _volumeManager.SaveUserVolumeAsync(participant.Id, volume);
    }

    public async Task<Volume> GetSavedUserVolumeAsync(CallParticipantModel participant)
    {
        return await _volumeManager.GetUserVolumeAsync(participant.Id);
    }

    internal void ParticipantConnected(RemoteParticipantState participant, GroupId groupId)
    {
        _logger.LogInformation("participant connected");

        if (CallChat?.Id != groupId) return;

        _logger.LogInformation("Adding participant");

        var user = _contactManager.GetOrCreateUserModel(participant);
        var participantModel = new CallParticipantModel(user)
        {
            ConnectionQuality = participant.ConnectionQuality,
            IsSpeaking = participant.IsSpeaking,
            MicrophoneEnabled = participant.MicrophoneOn
        };

        _participants.Add(participantModel);
    }

    internal void ParticipantDisconnected(RemoteParticipantState participant, GroupId groupId)
    {
        if (CallChat?.Id != groupId) return;

        _participants.Remove(participant.Id);
    }

    internal void ParticipantListReceived(RemoteParticipantState[] participants, GroupId groupId)
    {
        if (CallChat?.Id != groupId) return;

        using var scope = new NotificationScope(_participants);
        foreach (var participant in participants)
        {
            ParticipantConnected(participant, groupId);
        }
    }

    private Task OnIncomingCall(GroupId groupId, UserId initiatorId)
    {
        if (_chatManager.GetChannel(groupId) is not { } chat || chat.HasActiveCall)
        {
            return Task.CompletedTask;
        }

        chat.HasActiveCall = true;
        if (initiatorId == _contactManager.LocalUserModel.Id)
        {
            return Task.CompletedTask;
        }

        _logger.LogInformation("Incoming call from: {Caller}", groupId.Value);

        IncomingCall?.Invoke(chat);

        return Task.CompletedTask;
    }

    private Task OnCallAccepted(GroupId groupId, IChatUser accepting)
    {
        _logger.LogInformation("User {User} accepted call", accepting.Username);

        var acceptingUserModel = _contactManager.GetOrCreateUserModel(accepting);

        if (!_participants.ContainsKey(accepting.Id))
        {
            _participants[accepting.Id] = new CallParticipantModel(acceptingUserModel)
            {
                ConnectionQuality = ConnectionQuality.Unknown
            };
        }

        return Task.CompletedTask;
    }

    private Task OnCallDeclined(IChatUser declining, GroupId groupId)
    {
        _logger.LogInformation("Call {CallId} declined by {User}", groupId, declining.Username);
        return Task.CompletedTask;
    }

    private Task OnCallEnded(GroupId groupId)
    {
        if (_chatManager.GetChannel(groupId) is { } channel)
        {
            channel.HasActiveCall = false;
        }

        return Task.CompletedTask;
    }

    private Task OnCallFailed(string reason)
    {
        return Task.CompletedTask;
    }

    private async Task JoinRoomAsync(ChatModel chat, RoomTokenDto roomToken)
    {
        await TryInitialize();

        var joined = await _voiceChatInterop.JoinRoomAsync(roomToken, chat);
        if (!joined)
        {
            return;
        }

        CallChat = chat;
        chat.HasActiveCall = true;

        _participants.Add(LocalParticipant);
    }

    private async Task TryInitialize()
    {
        if (_voiceChatInterop.Initialized) return;

        var result = await _signalrService.GetRtcEndpoint();
        if (result.IsError) return;

        await _voiceChatInterop.InitializeAsync(result.Value);
    }
}
