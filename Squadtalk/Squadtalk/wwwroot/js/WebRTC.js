const lk = LivekitClient;
const Room = lk.Room;
const RoomEvent = lk.RoomEvent;
const ParticipantEvent = lk.ParticipantEvent;
const Track = lk.Track;
const Source = Track.Source;
const room = new Room({
    adaptiveStream: true,
    dynacast: true,
    disconnectOnPageLeave: true,
    publishDefaults: {
        simulcast: true,
        videoSimulcastLayers: [lk.VideoPresets.h216],
        dtx: true,
        red: true,
        forceStereo: false,
        screenShareEncoding: lk.ScreenSharePresets.h1080fps30.encoding,
        scalabilityMode: "L3T3"
    },
    videoCaptureDefaults: {
        resolution: lk.VideoPresets.h720.resolution
    }
});
room
    .on(RoomEvent.TrackSubscribed, handleTrackSubscribed)
    .on(RoomEvent.TrackUnsubscribed, handleTrackUnsubscribed)
    .on(RoomEvent.Disconnected, handleDisconnect)
    .on(RoomEvent.LocalTrackPublished, handleLocalTrackPublished)
    .on(RoomEvent.LocalTrackUnpublished, handleLocalTrackUnpublished)
    .on(RoomEvent.ParticipantConnected, participantConnected)
    .on(RoomEvent.ParticipantDisconnected, participantDisconnected)
    .on(RoomEvent.MediaDevicesChanged, handleDevicesChanged)
    .on(RoomEvent.MediaDevicesError, (e) => {
    const failure = lk.MediaDeviceFailure.getFailure(e);
    console.log('media device failure', failure);
})
    .on(RoomEvent.TrackMuted, handleTrackMuted)
    .on(RoomEvent.TrackUnmuted, handleTrackUnmuted);
let maximizeVideoFrame;
let maximizeVideoPlayer;
let dotnetObject;
let roomToken;
let serverAddress;
let maximizedTrackSid;
let maximizedParticipantIdentity;
let cameraFrontFacing = false;
let bitrateInterval;
export async function Init(object, url) {
    if (!object) {
        throw new Error("dotnet object is undefined");
    }
    dotnetObject = object;
    serverAddress = url;
    room.prepareConnection(serverAddress);
    maximizeVideoFrame = document.getElementById("maximized-video-container");
    maximizeVideoPlayer = document.getElementById("maximized-video");
}
export async function Start(token) {
    roomToken = token;
    try {
        await room.connect(serverAddress, roomToken);
        await room.localParticipant.setMicrophoneEnabled(true);
        const microphones = await Room.getLocalDevices("audioinput");
        await dotnetObject.invokeMethodAsync("MicrophonesUpdatedCallback", mapMediaDevices(microphones));
        const cameras = await Room.getLocalDevices("videoinput", false);
        await dotnetObject.invokeMethodAsync("CamerasUpdatedCallback", mapMediaDevices(cameras));
        await displayParticipant(room.localParticipant);
        bitrateInterval = setInterval(displayBitrate, 1000);
        const participant = room.localParticipant;
        participant
            .on(ParticipantEvent.TrackMuted, (pub) => displayParticipant(participant))
            .on(ParticipantEvent.TrackUnmuted, (pub) => displayParticipant(participant))
            .on(ParticipantEvent.IsSpeakingChanged, (isSpeaking) => displayParticipant(participant))
            .on(ParticipantEvent.ConnectionQualityChanged, (connectionQuality) => displayParticipant(participant));
        return true;
    }
    catch (e) {
        console.error(e);
        return false;
    }
}
export async function Stop() {
    if (bitrateInterval)
        clearInterval(bitrateInterval);
    await room.disconnect(true);
}
export async function SetMicrophoneEnabled(enabled) {
    await room.localParticipant.setMicrophoneEnabled(enabled);
    if (!enabled)
        return;
    const microphones = await Room.getLocalDevices("audioinput");
    await dotnetObject.invokeMethodAsync("MicrophonesUpdatedCallback", mapMediaDevices(microphones));
}
export async function SetCameraEnabled(enabled) {
    await room.localParticipant.setCameraEnabled(enabled);
    if (!enabled)
        return;
    const cameras = await Room.getLocalDevices("videoinput");
    await dotnetObject.invokeMethodAsync("CamerasUpdatedCallback", mapMediaDevices(cameras));
}
export async function SetScreenShareEnabled(enabled) {
    await room.localParticipant.setScreenShareEnabled(enabled);
}
export function ShowVideo(participantIdentity, videoMaximized) {
    const participant = room.getParticipantByIdentity(participantIdentity);
    if (!participant) {
        console.error("Participant is undefined");
        return;
    }
    const source = videoMaximized === 0 ? Source.Camera : Source.ScreenShare;
    const publication = participant.getTrackPublication(source);
    if (!publication) {
        console.error("Publication is undefined");
        return;
    }
    const track = publication.track;
    maximizedTrackSid = publication.trackSid;
    maximizedParticipantIdentity = participant.identity;
    maximizeVideoFrame.style.display = "block";
    publication.track.attach(maximizeVideoPlayer);
    if (source === Source.Camera) {
        attachVideo(track, participant, "camera");
        const otherTrack = participant.getTrackPublication(Source.ScreenShare)?.track;
        detachVideo(otherTrack, participant, "video");
    }
    else if (source === Source.ScreenShare) {
        attachVideo(track, participant, "video");
        const otherTrack = participant.getTrackPublication(Source.Camera)?.track;
        detachVideo(otherTrack, participant, "camera");
    }
}
export function MinimizeVideo() {
    const participant = room.getParticipantByIdentity(maximizedParticipantIdentity);
    if (!participant) {
        console.error("Participant is undefined");
        return;
    }
    const publication = participant.videoTrackPublications.get(maximizedTrackSid);
    if (!publication) {
        console.error("publication is undefined");
        return;
    }
    maximizedTrackSid = null;
    maximizedParticipantIdentity = null;
    maximizeVideoFrame.style.display = "none";
    publication.track.detach(maximizeVideoPlayer);
}
export function SwapCamera() {
    const cameraPublication = room?.localParticipant.getTrackPublication(Source.Camera);
    if (!cameraPublication) {
        return;
    }
    cameraFrontFacing = !cameraFrontFacing;
    const options = {
        resolution: lk.VideoPresets.h720.resolution,
        facingMode: cameraFrontFacing ? "user" : "environment"
    };
    cameraPublication.videoTrack?.restartTrack(options);
}
export function ChangeVolume(participantIdentity, volume, screenShare) {
    const participant = room.getParticipantByIdentity(participantIdentity);
    if (participant instanceof lk.RemoteParticipant) {
        const source = screenShare ? Source.ScreenShareAudio : Source.Microphone;
        participant.setVolume(volume / 100, source);
    }
}
export async function ChangeDevice(kind, id) {
    const mediaDeviceKind = kind === 0 ? "audioinput" : "videoinput";
    if (room) {
        await room.switchActiveDevice(mediaDeviceKind, id);
    }
}
function getPublication(participantIdentity, source) {
    const participant = room.getParticipantByIdentity(participantIdentity);
    if (!participant)
        return;
    return participant.getTrackPublication(source);
}
const mapMediaDevices = (devices) => devices.map(x => ({
    label: x.label,
    kind: x.kind,
    id: x.deviceId
}));
const mapParticipant = (participant) => {
    let totalBitrate = 0;
    for (const t of participant.trackPublications.values()) {
        if (t.track) {
            totalBitrate += t.track.currentBitrate;
        }
    }
    return ({
        Username: participant.identity,
        Remote: !participant.isLocal,
        MicrophoneOn: participant.isMicrophoneEnabled,
        CameraOn: participant.isCameraEnabled,
        ScreenShareOn: participant.isScreenShareEnabled,
        ConnectionQuality: participant.connectionQuality,
        Bitrate: Math.round(totalBitrate),
        Sid: participant.sid,
        IsSpeaking: participant.isSpeaking
    });
};
function attachVideo(track, participant, prefix) {
    if (!track)
        return;
    const container = document.getElementById(`${prefix}-container-${participant.sid}`);
    const player = document.getElementById(`${prefix}-${participant.sid}`);
    container.style.display = "block";
    track.attach(player);
}
function detachVideo(track, participant, prefix) {
    if (!track)
        return;
    const container = document.getElementById(`${prefix}-container-${participant.sid}`);
    const player = document.getElementById(`${prefix}-${participant.sid}`);
    container.style.display = "none";
    track.detach(player);
}
function handleTrackSubscribed(track, publication, participant) {
    const camera = participant.isCameraEnabled;
    const screenShare = participant.isScreenShareEnabled;
    const both = screenShare && camera;
    if (track.source === Source.Microphone) {
        const audioPlayer = document.getElementById(`audio-${participant.sid}`);
        track.attach(audioPlayer);
    }
    else if (track.source === Source.ScreenShareAudio) {
        const screenAudioPlayer = document.getElementById(`screen-audio-${participant.sid}`);
        track.attach(screenAudioPlayer);
    }
    else if (track.source === Source.Camera && !screenShare || both) {
        attachVideo(track, participant, "camera");
    }
    else if (track.source === Source.ScreenShare && !camera) {
        attachVideo(track, participant, "video");
    }
    displayParticipant(participant);
}
function handleTrackUnsubscribed(track, publication, participant) {
    track.detach();
    if (track.source === Source.Camera) {
        const cameraContainer = document.getElementById(`camera-container-${participant.sid}`);
        cameraContainer.style.display = "none";
    }
    else if (track.source === Source.ScreenShare) {
        const videoContainer = document.getElementById(`video-container-${participant.sid}`);
        videoContainer.style.display = "none";
    }
    if (track.sid === maximizedTrackSid) {
        maximizeVideoFrame.style.display = "none";
    }
    displayParticipant(participant);
}
function handleLocalTrackPublished(publication) {
    const track = publication.track;
    const participant = room.localParticipant;
    if (track.source === Source.Camera && !participant.isScreenShareEnabled) {
        attachVideo(track, participant, "camera");
    }
    else if (track.source === Source.ScreenShare && !participant.isCameraEnabled) {
        attachVideo(track, participant, "video");
    }
    displayParticipant(participant);
}
async function handleLocalTrackUnpublished(publication, participant) {
    const track = publication.track;
    track.detach();
    if (track.source === Source.Camera) {
        const cameraContainer = document.getElementById(`camera-container-${participant.sid}`);
        cameraContainer.style.display = "none";
    }
    else if (track.source === Source.ScreenShare) {
        const videoContainer = document.getElementById(`video-container-${participant.sid}`);
        videoContainer.style.display = "none";
    }
    if (track.sid === maximizedTrackSid) {
        maximizeVideoFrame.style.display = "none";
    }
    displayParticipant(participant);
}
function handleTrackMuted(publication, participant) {
    const track = publication.track;
    if (track.source === Source.Camera) {
        const cameraContainer = document.getElementById(`camera-container-${participant.sid}`);
        cameraContainer.style.display = "none";
    }
    else if (track.source === Source.ScreenShare) {
        const videoContainer = document.getElementById(`video-container-${participant.sid}`);
        videoContainer.style.display = "none";
    }
    if (track.sid === maximizedTrackSid) {
        maximizeVideoFrame.style.display = "none";
    }
    displayParticipant(participant);
}
function handleTrackUnmuted(publication, participant) {
    const track = publication.track;
    if (track.source === Source.Camera && !participant.isScreenShareEnabled) {
        const cameraContainer = document.getElementById(`camera-container-${participant.sid}`);
        cameraContainer.style.display = "block";
    }
    else if (track.source === Source.ScreenShare && !participant.isCameraEnabled) {
        const videoContainer = document.getElementById(`video-container-${participant.sid}`);
        videoContainer.style.display = "block";
    }
    displayParticipant(participant);
}
function handleDisconnect(reason) {
    dotnetObject.invokeMethodAsync("DisconnectedCallback", reason);
}
function participantConnected(participant) {
    console.log("participant", participant.identity, "connected", participant.metadata);
    console.log('tracks', participant.trackPublications);
    participant
        .on(ParticipantEvent.TrackMuted, (pub) => displayParticipant(participant))
        .on(ParticipantEvent.TrackUnmuted, (pub) => displayParticipant(participant))
        .on(ParticipantEvent.IsSpeakingChanged, (isSpeaking) => displayParticipant(participant))
        .on(ParticipantEvent.ConnectionQualityChanged, (connectionQuality) => displayParticipant(participant));
    dotnetObject.invokeMethodAsync("ParticipantConnectedCallback", mapParticipant(participant));
}
function participantDisconnected(participant) {
    console.log('participant', participant.identity, 'disconnected');
    dotnetObject.invokeMethodAsync("ParticipantDisconnectedCallback", mapParticipant(participant));
}
async function handleDevicesChanged() {
    const allDevices = await Room.getLocalDevices(null, false);
    allDevices.forEach(x => console.log('device', x.label, x.kind));
}
async function displayParticipant(participant) {
    await dotnetObject.invokeMethodAsync("DisplayParticipantCallback", mapParticipant(participant));
}
async function displayBitrate() {
    if (!room || room.state !== lk.ConnectionState.Connected) {
        return;
    }
    const participants = [...room.remoteParticipants.values()];
    participants.push(room.localParticipant);
    for (const participant of participants) {
        let totalBitrate = 0;
        for (const t of participant.trackPublications.values()) {
            if (t.track) {
                totalBitrate += t.track.currentBitrate;
            }
        }
        if (totalBitrate > 0) {
            console.log(`${participant.identity}: ${Math.round(totalBitrate / 1024).toLocaleString()} kbps`);
        }
    }
}
//# sourceMappingURL=WebRTC.js.map