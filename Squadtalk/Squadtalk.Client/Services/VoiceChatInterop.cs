using Microsoft.JSInterop;
using Shared.Data.TypedIds;
using Shared.Models;

namespace Squadtalk.Client.Services;

internal partial class VoiceChatServiceOld
{
    [JSInvokable]
    public void DisconnectedCallback(DisconnectReason reason)
    {
        ConnectedToVoiceCall = false;
        CallChannel = null;

        Disconnected?.Invoke(reason);
        CurrentChannelCallChanged?.Invoke();
    }

    [JSInvokable]
    public void ErrorCallback(string title, string message)
    {
        _logger.LogError("Error {Title} {Message}", title, message);

        Error?.Invoke(title, message);
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
        MicrophoneListUpdated?.Invoke();
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
        CameraListUpdated?.Invoke();
    }

    [JSInvokable]
    public void ParticipantUpdatedCallback(CallParticipantModelOld participant, GroupId groupId)
    {
        if (!_participants.TryAdd(participant.Id, participant))
        {
            var existingParticipant = _participants[participant.Id];

            existingParticipant.MicrophoneOn = participant.MicrophoneOn;
            existingParticipant.CameraOn = participant.CameraOn;
            existingParticipant.ScreenShareOn = participant.ScreenShareOn;
            existingParticipant.ConnectionQuality = participant.ConnectionQuality;
            existingParticipant.IsSpeaking = participant.IsSpeaking;
        }

        ParticipantUpdated?.Invoke(participant.Id, _chatManager.GetRequiredChannel(groupId));
    }

    [JSInvokable]
    public void ParticipantListReceivedCallback(List<CallParticipantModelOld> participants, GroupId groupId)
    {
        foreach (var participant in participants)
        {
            _participants[participant.Id] = participant;
        }

        ParticipantListUpdated?.Invoke(_chatManager.GetRequiredChannel(groupId));
    }

    [JSInvokable]
    public void ParticipantConnectedCallback(CallParticipantModelOld participant, GroupId groupId)
    {
        _participants[participant.Id] = participant;
        ParticipantListUpdated?.Invoke(_chatManager.GetRequiredChannel(groupId));
    }

    [JSInvokable]
    public void ParticipantDisconnectedCallback(CallParticipantModelOld participant, GroupId groupId)
    {
        _participants.Remove(participant.Id);
        ParticipantListUpdated?.Invoke(_chatManager.GetRequiredChannel(groupId));
    }
}
