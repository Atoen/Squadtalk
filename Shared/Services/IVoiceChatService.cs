using System.Diagnostics.CodeAnalysis;
using Shared.Communication;
using Shared.Data.TypedIds;
using Shared.Models;

namespace Shared.Services;

public delegate void ErrorNotificationHandler(string title, string message);

public interface IVoiceChatService
{
    [MemberNotNullWhen(true, nameof(CallChannel))]
    bool ConnectedToVoiceCall { get; }

    bool ActiveCallOnCurrentChannel { get; }

    ChannelModel? CallChannel { get; }

    ChannelModel? CurrentChannel { get; }

    bool MicrophoneEnabled { get; }

    bool CameraEnabled { get; }

    bool ScreenShareEnabled { get; }

    bool MicrophoneAvailable { get; }

    bool CameraAvailable { get; }

    IEnumerable<MediaDeviceModel> Microphones { get; }

    IEnumerable<MediaDeviceModel> Cameras { get; }

    IEnumerable<CallParticipantModel> ActiveCallParticipants { get; }

    event Action? OnMicrophoneListUpdated;
    event Action? OnCameraListUpdated;
    event ErrorNotificationHandler? OnError;
    event Action<DisconnectReason>? OnDisconnected;
    event Action? OnCurrentChannelCallChanged;
    event Func<ChannelModel, Task>? OnCallIncoming;
    event Action<ChannelModel>? OnCallEnded;
    event Action<ChannelModel>? OnParticipantsUpdated;

    Task InitializeAsync();

    Task StartCallAsync(ChannelId channelId);

    Task AcceptCallAsync(ChannelId channelId);

    Task DeclineCallAsync(ChannelId channelId);

    Task LeaveCallAsync();

    Task ChangeVolumeAsync(CallParticipantModel participant, Volume volume, AudioSource audioSource = AudioSource.Microphone);

    Task SwapCameraAsync();

    Task ShowVideoAsync(CallParticipantModel participant, VideoSource videoSource);

    Task MinimizeVideoAsync();

    Task SelectMicrophoneAsync(MediaDeviceModel microphone);

    Task SelectCameraAsync(MediaDeviceModel camera);

    Task ToggleMicrophoneAsync();

    Task ToggleCameraAsync();

    Task ToggleScreenShareAsync();

    Task<bool> CheckIfChannelHasActiveCallAsync(ChannelModel channel);
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

public readonly record struct Volume(int Value)
{
    public int Value { get; init; } = Value is < 0 or > 100
        ? throw new ArgumentOutOfRangeException(nameof(Value),
            "Volume must be not negative and lass than or equal to 100.")
        : Value;
}
