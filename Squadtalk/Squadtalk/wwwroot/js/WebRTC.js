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
        videoSimulcastLayers: [lk.VideoPresets.h216, lk.VideoPresets.h90],
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
    .on(RoomEvent.TrackSubscribed, trackSubscribed)
    .on(RoomEvent.TrackUnsubscribed, trackUnsubscribed)
    .on(RoomEvent.Disconnected, disconnected)
    .on(RoomEvent.LocalTrackPublished, localTrackPublished)
    .on(RoomEvent.LocalTrackUnpublished, localTrackUnpublished)
    .on(RoomEvent.ParticipantConnected, participantConnected)
    .on(RoomEvent.ParticipantDisconnected, participantDisconnected)
    .on(RoomEvent.MediaDevicesChanged, mediaDevicesChanged)
    .on(RoomEvent.TrackMuted, trackMuted)
    .on(RoomEvent.TrackUnmuted, trackUnmuted)
    .on(RoomEvent.LocalAudioSilenceDetected, localAudioSilenceDetected)
    .on(RoomEvent.MediaDevicesError, mediaDeviceError);
let maximizeVideoFrame;
let maximizeVideoPlayer;
let dotnetObject;
let serverAddress;
let maximizedTrackSid;
let maximizedParticipantIdentity;
let cameraFrontFacing = false;
let bitrateInterval;
let microphoneEnabled;
let cameraEnabled;
let screenShareEnabled;
export async function Init(object, url) {
    if (!object) {
        throw new Error("dotnet object is undefined");
    }
    dotnetObject = object;
    serverAddress = url;
    room.prepareConnection(serverAddress);
}
export async function Start(token) {
    try {
        await room.connect(serverAddress, token);
    }
    catch {
        await error("Failed to connect to the room", "Unable to connect to the room");
        return false;
    }
    const localParticipant = room.localParticipant;
    localParticipant
        .on(ParticipantEvent.TrackMuted, updateLocalState)
        .on(ParticipantEvent.TrackUnmuted, updateLocalState)
        .on(ParticipantEvent.IsSpeakingChanged, updateLocalState)
        .on(ParticipantEvent.ConnectionQualityChanged, updateLocalState);
    setInterval(debugRemoteParticipants, 1000);
    updateLocalState();
    updateJoinedParticipants();
    return true;
}
export async function Stop() {
    if (bitrateInterval)
        clearInterval(bitrateInterval);
    if (maximizedParticipantIdentity)
        MinimizeVideo();
    await room.disconnect(true);
}
export async function ToggleMicrophoneEnabled() {
    await room.localParticipant.setMicrophoneEnabled(!microphoneEnabled);
    microphoneEnabled = room.localParticipant.isMicrophoneEnabled;
    if (microphoneEnabled) {
        const microphones = await Room.getLocalDevices("audioinput");
        await dotnetObject.invokeMethodAsync("MicrophonesUpdatedCallback", mapMediaDevices(microphones));
    }
    return microphoneEnabled;
}
export async function ToggleCameraEnabled() {
    await room.localParticipant.setCameraEnabled(!cameraEnabled);
    cameraEnabled = room.localParticipant.isCameraEnabled;
    if (cameraEnabled) {
        const cameras = await Room.getLocalDevices("videoinput");
        await dotnetObject.invokeMethodAsync("CamerasUpdatedCallback", mapMediaDevices(cameras));
    }
    return cameraEnabled;
}
export async function ToggleScreenShareEnabled() {
    await room.localParticipant.setScreenShareEnabled(!screenShareEnabled);
    screenShareEnabled = room.localParticipant.isScreenShareEnabled;
    return screenShareEnabled;
}
export function MaximizeVideo(participantIdentity, videoMaximized) {
    const participant = room.getParticipantByIdentity(participantIdentity);
    if (!participant) {
        console.error("Participant is undefined");
        return;
    }
    const source = videoMaximized === 0 ? Source.Camera : Source.ScreenShare;
    const publication = participant.getTrackPublication(source);
    const track = publication.track;
    if (!publication || !track) {
        console.error("Track is undefined");
        return;
    }
    const [container, player] = getMaximizedVideoContainerAndPlayer();
    if (container)
        container.style.display = "block";
    if (player)
        track.attach(player);
}
export function MinimizeVideo() {
    const [container, player] = getMaximizedVideoContainerAndPlayer();
    if (container)
        container.style.display = "none";
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
    maximizeVideoFrame.style.display = "none";
    publication.track.detach(maximizeVideoPlayer);
    maximizedTrackSid = null;
    maximizedParticipantIdentity = null;
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
export function ChangeDevice(kind, id) {
    const mediaDeviceKind = kind === 0 ? "audioinput" : "videoinput";
    room?.switchActiveDevice(mediaDeviceKind, id);
}
const debugRemoteParticipants = () => {
    for (const re of room.remoteParticipants.values()) {
        console.log(`${re.name}: ${re.connectionQuality}`);
    }
    const mapped = Array.from(room.remoteParticipants.values())
        .map(mapParticipant);
    dotnetObject.invokeMethodAsync("ParticipantListReceivedCallback", mapped, room.name);
};
const getParticipantAudioElement = (participant) => {
    const id = `audio-${participant.sid}`;
    const element = document.getElementById(id);
    if (element)
        return element;
    const newElement = document.createElement('audio');
    newElement.id = id;
    return newElement;
};
const removeParticipantAudioElement = (participant) => {
    const id = `audio-${participant.sid}`;
    const element = document.getElementById(id);
    element?.parentElement?.removeChild(element);
};
const getMaximizedVideoContainerAndPlayer = () => {
    const container = document.getElementById("maximized-video-container");
    if (!container) {
        console.error("Maximized video container is missing");
    }
    const player = document.getElementById("maximized-video");
    if (!player) {
        console.error("Maximized video player is missing");
    }
    return [container, player];
};
const error = (title, message) => dotnetObject.invokeMethodAsync("ErrorCallback", title, message);
const updateJoinedParticipants = () => {
    const mapped = Array.from(room.remoteParticipants.values())
        .map(mapParticipant);
    dotnetObject.invokeMethodAsync("ParticipantListReceivedCallback", mapped, room.name);
};
const updateLocalState = () => {
    const localParticipant = room?.localParticipant;
    if (!localParticipant) {
        console.error("Local participant is undefined");
        return;
    }
    const state = ({
        MicrophoneOn: localParticipant.isMicrophoneEnabled,
        CameraOn: localParticipant.isCameraEnabled,
        ScreenShareOn: localParticipant.isScreenShareEnabled,
        ConnectionQuality: localParticipant.connectionQuality,
    });
    dotnetObject.invokeMethodAsync("LocalParticipantStateUpdatedCallback", state);
    updateParticipant(localParticipant);
};
const mapParticipant = (participant) => {
    let totalBitrate = 0;
    for (const t of participant.trackPublications.values()) {
        if (t.track)
            totalBitrate += t.track.currentBitrate;
    }
    return ({
        Username: participant.name,
        Id: participant.identity,
        Sid: participant.sid,
        Remote: !participant.isLocal,
        MicrophoneOn: participant.isMicrophoneEnabled,
        CameraOn: participant.isCameraEnabled,
        ScreenShareOn: participant.isScreenShareEnabled,
        ConnectionQuality: participant.connectionQuality,
        Bitrate: Math.round(totalBitrate),
        IsSpeaking: participant.isSpeaking
    });
};
const updateParticipant = (participant) => dotnetObject.invokeMethodAsync("ParticipantUpdatedCallback", mapParticipant(participant), room.name);
const enableMicrophoneOnJoin = async () => {
    try {
        const publication = await room.localParticipant.setMicrophoneEnabled(true);
        if (!publication)
            return false;
        const microphones = await Room.getLocalDevices("audioinput");
        await dotnetObject.invokeMethodAsync("MicrophonesUpdatedCallback", mapMediaDevices(microphones));
        const cameras = await Room.getLocalDevices("videoinput", false);
        await dotnetObject.invokeMethodAsync("CamerasUpdatedCallback", mapMediaDevices(cameras));
        return true;
    }
    catch {
        return false;
    }
};
const mapMediaDevices = (devices) => devices.map(x => ({
    label: x.label,
    kind: x.kind,
    id: x.deviceId
}));
function trackSubscribed(track, publication, participant) {
    if (track.source === Source.Microphone) {
        const audioPlayer = getParticipantAudioElement(participant);
        track.attach(audioPlayer);
    }
    updateParticipant(participant);
}
function trackUnsubscribed(track, publication, participant) {
    track.detach();
    if (track.source == Source.Microphone)
        removeParticipantAudioElement(participant);
    updateParticipant(participant);
}
function disconnected(reason) {
    dotnetObject.invokeMethodAsync("DisconnectedCallback", reason, room.name);
}
function localTrackPublished(publication) {
    updateParticipant(room.localParticipant);
}
function localTrackUnpublished(publication, participant) {
    publication.track?.detach();
    updateParticipant(room.localParticipant);
}
function participantConnected(participant) {
    participant
        .on(ParticipantEvent.TrackMuted, () => updateParticipant(participant))
        .on(ParticipantEvent.TrackUnmuted, () => updateParticipant(participant))
        .on(ParticipantEvent.IsSpeakingChanged, () => updateParticipant(participant))
        .on(ParticipantEvent.ConnectionQualityChanged, () => {
        console.log(`Connection quality for ${participant.name}: ${participant.connectionQuality}`);
        return updateParticipant(participant);
    });
    dotnetObject.invokeMethodAsync("ParticipantConnectedCallback", mapParticipant(participant), room.name);
}
function participantDisconnected(participant) {
    dotnetObject.invokeMethodAsync("ParticipantDisconnectedCallback", mapParticipant(participant), room.name);
}
function mediaDevicesChanged() {
    Room.getLocalDevices(null, false)
        .then(r => r.forEach(x => console.log('device', x.label, x.kind)));
}
function trackMuted(publication, participant) {
    if (participant.isLocal) {
        updateLocalState();
    }
    else {
        updateParticipant(participant);
    }
}
function trackUnmuted(publication, participant) {
    if (participant.isLocal) {
        updateLocalState();
    }
    else {
        updateParticipant(participant);
    }
}
function localAudioSilenceDetected() {
}
function mediaDeviceError(error) {
    const failure = lk.MediaDeviceFailure.getFailure(error);
    dotnetObject.invokeMethodAsync("ErrorCallback", "Media device error", failure);
}
//# sourceMappingURL=WebRTC.js.map