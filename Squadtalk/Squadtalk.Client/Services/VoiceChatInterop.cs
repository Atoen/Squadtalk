using Microsoft.JSInterop;
using Shared.Data.TypedIds;
using Shared.Models;

namespace Squadtalk.Client.Services;

public partial class VoiceChatService
{
    [JSInvokable]
    public void DisconnectedCallback(DisconnectReason reason, ChannelId channelId)
    {
        ConnectedToVoiceCall = false;
        CallChannel = null;

        OnDisconnected?.Invoke(reason);
        OnCurrentChannelCallChanged?.Invoke();
    }

    [JSInvokable]
    public void ErrorCallback(string title, string message)
    {
        _logger.LogError("Error {Title} {Message}", title, message);

        OnError?.Invoke(title, message);
    }

    [JSInvokable]
    public void LocalParticipantStateUpdatedCallback(ParticipantState localParticipantState)
    {
        MicrophoneEnabled = localParticipantState.MicrophoneOn;
        CameraEnabled = localParticipantState.CameraOn;
        ScreenShareEnabled = localParticipantState.ScreenShareOn;
        ConnectionQuality = localParticipantState.ConnectionQuality;

        LocalParticipantStateUpdated?.Invoke();
    }

    [JSInvokable]
    public void MicrophonesUpdatedCallback(List<MediaDeviceModel> microphones)
    {
        if (MediaDevicesMatches(_microphones, microphones))
        {
            _logger.LogDebug("Microphones are matching. Skipping update");
            return;
        }

        _microphones.Clear();
        _microphones.AddRange(microphones);
        OnMicrophoneListUpdated?.Invoke();
    }

    [JSInvokable]
    public void CamerasUpdatedCallback(List<MediaDeviceModel> cameras)
    {
        if (MediaDevicesMatches(_cameras, cameras))
        {
            _logger.LogDebug("Cameras are matching. Skipping update");
            return;
        }

        _cameras.Clear();
        _cameras.AddRange(cameras);
        OnCameraListUpdated?.Invoke();
    }

    [JSInvokable]
    public void ParticipantUpdatedCallback(CallParticipantModel participant, ChannelId channelId)
    {
        _participants[participant.Id] = participant;
        OnParticipantsUpdated?.Invoke(_chatService.GetRequiredChannel(channelId));
    }

    [JSInvokable]
    public void ParticipantListReceivedCallback(List<CallParticipantModel> participants, ChannelId channelId)
    {
        foreach (var participant in participants)
        {
            _participants[participant.Id] = participant;
        }

        OnParticipantsUpdated?.Invoke(_chatService.GetRequiredChannel(channelId));
    }

    [JSInvokable]
    public void ParticipantConnectedCallback(CallParticipantModel participant, ChannelId channelId)
    {
        _participants[participant.Id] = participant;
        OnParticipantsUpdated?.Invoke(_chatService.GetRequiredChannel(channelId));
    }

    [JSInvokable]
    public void ParticipantDisconnectedCallback(CallParticipantModel participant, ChannelId channelId)
    {
        _participants.Remove(participant.Id);
        OnParticipantsUpdated?.Invoke(_chatService.GetRequiredChannel(channelId));
    }
}
