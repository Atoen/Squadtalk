const lk = LivekitClient;
const room = new lk.Room({
    adaptiveStream: true,
    dynacast: true,
    disconnectOnPageLeave: true
});
room
    .on(lk.RoomEvent.TrackSubscribed, handleTrackSubscribed)
    .on(lk.RoomEvent.TrackUnsubscribed, handleTrackUnsubscribed)
    .on(lk.RoomEvent.ActiveSpeakersChanged, handleActiveSpeakerChange)
    .on(lk.RoomEvent.Disconnected, handleDisconnect)
    .on(lk.RoomEvent.LocalTrackPublished, handleLocalTrackPublished)
    .on(lk.RoomEvent.LocalTrackUnpublished, handleLocalTrackUnpublished)
    .on(lk.RoomEvent.ParticipantConnected, participantConnected)
    .on(lk.RoomEvent.ParticipantDisconnected, participantDisconnected)
    .on(lk.RoomEvent.MediaDevicesChanged, handleDevicesChanged)
    .on(lk.RoomEvent.MediaDevicesError, (e) => {
    const failure = lk.MediaDeviceFailure.getFailure(e);
    console.log('media device failure', failure);
});
let audioInputDevices;
let audioOutputDevices;
let videoInputDevices;
let localVideoFrame;
let localVideoPlayer;
let remoteVideoFrame;
let remoteVideoPlayer;
let dotnetObject;
let roomToken;
let serverAddress;
export async function Init(object, url) {
    if (!object) {
        throw new Error("dotnet object is undefined");
    }
    dotnetObject = object;
    serverAddress = url;
    room.prepareConnection(serverAddress);
    localVideoFrame = document.getElementById("local-video-container");
    localVideoPlayer = document.getElementById("local-video");
    remoteVideoFrame = document.getElementById("remote-video-container");
    remoteVideoPlayer = document.getElementById("remote-video");
}
export async function Start(token) {
    roomToken = token;
    try {
        await room.connect(serverAddress, roomToken);
        await room.localParticipant.setMicrophoneEnabled(true);
        const microphones = await lk.Room.getLocalDevices("audioinput");
        await dotnetObject.invokeMethodAsync("OnMicrophonesUpdated", mapMediaDevices(microphones));
        return true;
    }
    catch (e) {
        await dotnetObject.invokeMethodAsync("OnError", e.message);
        console.error(e);
        return false;
    }
}
export async function Stop() {
    await room.disconnect(true);
}
export async function SetMicrophoneEnabled(enabled) {
    await room.localParticipant.setMicrophoneEnabled(enabled);
    if (!enabled)
        return;
    const microphones = await lk.Room.getLocalDevices("audioinput");
    await dotnetObject.invokeMethodAsync("OnMicrophonesUpdated", mapMediaDevices(microphones));
}
export async function SetCameraEnabled(enabled) {
    await room.localParticipant.setCameraEnabled(enabled);
    if (!enabled)
        return;
    const cameras = await lk.Room.getLocalDevices("videoinput");
    await dotnetObject.invokeMethodAsync("OnCamerasUpdated", mapMediaDevices(cameras));
}
export async function SetScreenShareEnabled(enabled) {
    await room.localParticipant.setScreenShareEnabled(enabled);
}
const mapMediaDevices = (devices) => devices.map(x => ({
    label: x.label,
    kind: x.kind,
    id: x.deviceId
}));
function handleTrackSubscribed(track, publication, participant) {
    if (track.kind !== lk.Track.Kind.Video)
        return;
    console.log("Subscribed to track", publication.trackSid, participant.identity);
    remoteVideoFrame.style.display = "block";
    track.attach(remoteVideoPlayer);
}
function handleTrackUnsubscribed(track, publication, participant) {
    remoteVideoFrame.style.display = "none";
    track.detach();
}
function handleLocalTrackPublished(publication) {
    const track = publication.track;
    if (track.kind !== lk.Track.Kind.Video)
        return;
    localVideoFrame.style.display = "block";
    track.attach(localVideoPlayer);
}
async function handleLocalTrackUnpublished(publication, participant) {
    localVideoFrame.style.display = "none";
    publication.track.detach();
    await dotnetObject.invokeMethodAsync("OnLocalTrackUnpublished");
}
function handleActiveSpeakerChange(speakers) {
}
function handleDisconnect() {
    console.log('disconnected from room');
}
function participantConnected(participant) {
    console.log("participant", participant.identity, "connected", participant.metadata);
    console.log('tracks', participant.trackPublications);
    participant
        .on(lk.ParticipantEvent.TrackMuted, (pub) => {
        console.log('track was muted', pub.trackSid, participant.identity);
        displayParticipant(participant);
    })
        .on(lk.ParticipantEvent.TrackUnmuted, (pub) => {
        console.log('track was unmuted', pub.trackSid, participant.identity);
        displayParticipant(participant);
    })
        .on(lk.ParticipantEvent.IsSpeakingChanged, () => {
        displayParticipant(participant);
    })
        .on(lk.ParticipantEvent.ConnectionQualityChanged, () => {
        displayParticipant(participant);
    });
}
function participantDisconnected(participant) {
    console.log('participant', participant.identity, 'disconnected');
}
async function handleDevicesChanged() {
    const allDevices = await lk.Room.getLocalDevices(null, false);
    audioInputDevices = allDevices.filter(device => device.kind === 'audioinput');
    audioOutputDevices = allDevices.filter(device => device.kind === 'audiooutput');
    videoInputDevices = allDevices.filter(device => device.kind === 'videoinput');
    allDevices.forEach(x => console.log('device', x.label, x.kind));
}
function displayParticipant(participant) {
}
//# sourceMappingURL=WebRTC.js.map