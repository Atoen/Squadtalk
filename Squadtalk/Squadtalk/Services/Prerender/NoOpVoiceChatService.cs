using Shared.Data.TypedIds;
using Shared.Models;
using Shared.Services;

namespace Squadtalk.Services.Prerender;

internal class NoOpVoiceChatService : IVoiceChatService
{
    public bool ConnectedToVoiceCall { get; }
    public bool ConnectedToVoiceCallOnCurrentChannel { get; }
    public bool ActiveCallOnCurrentChannel { get; }
    public ChannelModel? CallChannel { get; }
    public ChannelModel? CurrentChannel { get; }
    public bool MicrophoneEnabled { get; }
    public bool CameraEnabled { get; }
    public bool ScreenShareEnabled { get; }
    public bool MicrophoneAvailable { get; }
    public bool CameraAvailable { get; }
    public ConnectionQuality ConnectionQuality { get; }
    public IEnumerable<MediaDeviceModel> Microphones { get; }
    public IEnumerable<MediaDeviceModel> Cameras { get; }
    public IEnumerable<CallParticipantModel> ActiveCallParticipants { get; }
    public event Action? MicrophoneListUpdated;
    public event Action? CameraListUpdated;
    public event ErrorNotificationHandler? Error;
    public event Action<DisconnectReason>? Disconnected;
    public event Action? CurrentChannelCallChanged;
    public event Func<ChannelModel, Task>? CallIncoming;
    public event Action<ChannelModel>? CallEnded;
    public event Action<ChannelModel>? ParticipantListUpdated;
    public event Action<UserId, ChannelModel>? ParticipantUpdated;
    public event Action? LocalParticipantStateUpdated;
    public Task InitializeAsync() => throw new NotImplementedException();
    public Task StartCallAsync(ChannelId channelId) => throw new NotImplementedException();
    public Task AcceptCallAsync(ChannelId channelId) => throw new NotImplementedException();
    public Task DeclineCallAsync(ChannelId channelId) => throw new NotImplementedException();
    public Task LeaveCallAsync() => throw new NotImplementedException();
    public Task ChangeVolumeAsync(CallParticipantModel participant, Volume volume, AudioSource audioSource = AudioSource.Microphone) => throw new NotImplementedException();
    public Task<Volume> GetUserVolumeAsync(CallParticipantModel participantModel) => throw new NotImplementedException();
    public Task SwapCameraAsync() => throw new NotImplementedException();
    public Task MaximizeVideoAsync(CallParticipantModel participant, VideoSource videoSource) => throw new NotImplementedException();
    public Task MinimizeVideoAsync() => throw new NotImplementedException();
    public Task SelectMicrophoneAsync(MediaDeviceModel microphone) => throw new NotImplementedException();
    public Task SelectCameraAsync(MediaDeviceModel camera) => throw new NotImplementedException();
    public Task ToggleMicrophoneAsync() => throw new NotImplementedException();
    public Task ToggleCameraAsync() => throw new NotImplementedException();
    public Task ToggleScreenShareAsync() => throw new NotImplementedException();
    public Task<bool> CheckIfChannelHasActiveCallAsync(ChannelModel channel) => Task.FromResult(false);
}
