const room = new LivekitClient.Room({
    adaptiveStream: true,
    dynacast: true,
});
const url = "ws://127.0.0.1:1230/jajo";
let token;
room
    .on(LivekitClient.RoomEvent.TrackSubscribed, handleTrackSubscribed)
    .on(LivekitClient.RoomEvent.TrackUnsubscribed, handleTrackUnsubscribed)
    .on(LivekitClient.RoomEvent.ActiveSpeakersChanged, handleActiveSpeakerChange)
    .on(LivekitClient.RoomEvent.Disconnected, handleDisconnect)
    .on(LivekitClient.RoomEvent.LocalTrackUnpublished, handleLocalTrackUnpublished);
let dotnetObject;
let mediaStream;
let videoElement;
let videoFrame;
export async function Init(object) {
    if (!object) {
        throw new Error("dotnet object is undefined");
    }
    dotnetObject = object;
    videoFrame = document.getElementById("video-container");
    videoElement = document.getElementById("local-video");
}
function handleTrackSubscribed(track, publication, participant) {
    if (track.kind === Track.Kind.Video || track.kind === Track.Kind.Audio) {
        const element = track.attach();
        videoFrame.appendChild(element);
    }
}
function handleTrackUnsubscribed(track, publication, participant) {
    track.detach();
}
function handleLocalTrackUnpublished(publication, participant) {
    publication.track.detach();
}
function handleActiveSpeakerChange(speakers) {
}
function handleDisconnect() {
    console.log('disconnected from room');
}
export async function Start(token) {
    token2 = token;
    await room.prepareConnection(url, token2);
    await room.connect('ws://127.0.0.1:1230/jajo', token2);
    const participant = room.localParticipant;
    await participant.setScreenShareEnabled(true);
    return true;
}
export async function Stop() {
    const participant = room.localParticipant;
    await participant.setScreenShareEnabled(false);
}
//# sourceMappingURL=WebRTC.js.map