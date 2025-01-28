using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Shared.Data.JsonConverters;
using Shared.Data.TypedIds;
using Shared.Models;

namespace Shared.Services;

public delegate void ErrorNotificationHandler(string title, string message);

public interface IVoiceChatService
{
    [MemberNotNullWhen(true, nameof(CallChannel))]
    bool ConnectedToVoiceCall { get; }

    [MemberNotNullWhen(true, nameof(CallChannel))]
    bool ConnectedToVoiceCallOnCurrentChannel { get; }

    bool ActiveCallOnCurrentChannel { get; }

    ChatModel? CallChannel { get; }

    ChatModel? CurrentChannel { get; }

    bool MicrophoneEnabled { get; }

    bool CameraEnabled { get; }

    bool ScreenShareEnabled { get; }

    bool MicrophoneAvailable { get; }

    bool CameraAvailable { get; }

    ConnectionQuality ConnectionQuality { get; }

    IEnumerable<MediaDeviceModel> Microphones { get; }

    IEnumerable<MediaDeviceModel> Cameras { get; }

    IEnumerable<CallParticipantModel> ActiveCallParticipants { get; }

    event Action? MicrophoneListUpdated;
    event Action? CameraListUpdated;
    event ErrorNotificationHandler? Error;
    event Action<DisconnectReason>? Disconnected;
    event Action? CurrentChannelCallChanged;
    event Func<ChatModel, Task>? CallIncoming;
    event Action<ChatModel>? CallEnded;
    event Action<ChatModel>? ParticipantListUpdated;
    event Action<UserId, ChatModel>? ParticipantUpdated;
    event Action? LocalParticipantStateUpdated;

    Task InitializeAsync();

    Task StartCallAsync(GroupId groupId);

    Task AcceptCallAsync(GroupId groupId);

    Task DeclineCallAsync(GroupId groupId);

    Task LeaveCallAsync();

    Task ChangeVolumeAsync(CallParticipantModel participant, Volume volume, AudioSource audioSource = AudioSource.Microphone);

    Task<Volume> GetUserVolumeAsync(CallParticipantModel participantModel);

    Task SwapCameraAsync();

    Task MaximizeVideoAsync(CallParticipantModel participant, VideoSource videoSource);

    Task MinimizeVideoAsync();

    Task SelectMicrophoneAsync(MediaDeviceModel microphone);

    Task SelectCameraAsync(MediaDeviceModel camera);

    Task ToggleMicrophoneAsync();

    Task ToggleCameraAsync();

    Task ToggleScreenShareAsync();

    Task<bool> CheckIfChannelHasActiveCallAsync(ChatModel chat);
}

public enum AudioSource
{
    Microphone,
    ScreenShareAudio
}

public enum VideoSource
{
    Camera,
    ScreenShare
}

public enum InputDevice
{
    Microphone,
    Camera
}

[JsonConverter(typeof(VolumeConverter))]
public readonly record struct Volume(int Value)
{
    public int Value { get; } = Value is < 0 or > 100
        ? throw new ArgumentOutOfRangeException(nameof(Value),
            "Volume must be not negative and not greater than 100.")
        : Value;

    public static Volume Full => new(100);

    public static implicit operator int(Volume volume) => volume.Value;
}
