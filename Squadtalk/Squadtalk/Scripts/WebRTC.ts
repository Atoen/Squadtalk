// noinspection JSUnusedGlobalSymbols

// @ts-nocheck
const lk = LivekitClient;

interface DotnetObject {
    invokeMethodAsync(identifier: string, ...args: any): Promise<void>
    invokeMethod(identifier: string, ...args: any): void
}

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
    .on(lk.RoomEvent.MediaDevicesError, (e: Error) => {
        const failure = lk.MediaDeviceFailure.getFailure(e);
        console.log('media device failure', failure);
    })

// const url = "wss://192.168.1.103:1230/jajo";
// room.prepareConnection(url);

let audioInputDevices: MediaDeviceInfo[];
let audioOutputDevices: MediaDeviceInfo[];
let videoInputDevices: MediaDeviceInfo[];

let localVideoFrame: HTMLElement;
let localVideoPlayer: HTMLVideoElement;
let remoteVideoFrame: HTMLElement;
let remoteVideoPlayer: HTMLVideoElement;

let dotnetObject: DotnetObject

let roomToken: string;
let serverAddress: string;

export async function Init(object: DotnetObject, url: string) {
    if (!object) {
        throw new Error("dotnet object is undefined")
    }

    dotnetObject = object;
    serverAddress = url;

    room.prepareConnection(serverAddress);

    localVideoFrame = document.getElementById("local-video-container");
    localVideoPlayer = document.getElementById("local-video") as HTMLVideoElement;

    remoteVideoFrame = document.getElementById("remote-video-container");
    remoteVideoPlayer = document.getElementById("remote-video") as HTMLVideoElement;
}

export async function Start(token: string): Promise<boolean> {
    roomToken = token;

    try {
        await room.connect(serverAddress, roomToken);
        await room.localParticipant.setMicrophoneEnabled(true);
        const microphones = await lk.Room.getLocalDevices("audioinput");
        await dotnetObject.invokeMethodAsync("OnMicrophonesUpdated", mapMediaDevices(microphones));
        return true;
    } catch (e: Error) {
        await dotnetObject.invokeMethodAsync("OnError", e.message);
        console.error(e);
        return false;
    }
}

export async function Stop() {
    await room.disconnect(true);
}

export async function SetMicrophoneEnabled(enabled: boolean) {
    await room.localParticipant.setMicrophoneEnabled(enabled);
    if (!enabled) return;

    const microphones = await lk.Room.getLocalDevices("audioinput");
    await dotnetObject.invokeMethodAsync("OnMicrophonesUpdated", mapMediaDevices(microphones));
}

export async function SetCameraEnabled(enabled: boolean) {
    await room.localParticipant.setCameraEnabled(enabled);

    if (!enabled) return;

    const cameras = await lk.Room.getLocalDevices("videoinput");
    await dotnetObject.invokeMethodAsync("OnCamerasUpdated", mapMediaDevices(cameras));
}

export async function SetScreenShareEnabled(enabled: boolean) {
    await room.localParticipant.setScreenShareEnabled(enabled);
}

const mapMediaDevices = (devices: MediaDeviceInfo[]) => devices.map(x => ({
    label: x.label,
    kind: x.kind,
    id: x.deviceId
}));

function handleTrackSubscribed(
    track: lk.RemoteTrack,
    publication: lk.RemoteTrackPublication,
    participant: lk.RemoteParticipant,
) {

    if (track.kind !== lk.Track.Kind.Video) return;

    console.log("Subscribed to track", publication.trackSid, participant.identity)

    remoteVideoFrame.style.display = "block";
    track.attach(remoteVideoPlayer);
}

function handleTrackUnsubscribed(
    track: lk.RemoteTrack,
    publication: lk.RemoteTrackPublication,
    participant: lk.RemoteParticipant,
) {

    remoteVideoFrame.style.display = "none";


    // remove tracks from all attached elements
    track.detach();
}

function handleLocalTrackPublished(publication: lk.LocalTrackPublication) {

    const track = publication.track;
    if (track.kind !== lk.Track.Kind.Video) return;

    localVideoFrame.style.display = "block";
    track.attach(localVideoPlayer);
}

async function handleLocalTrackUnpublished(
    publication: lk.LocalTrackPublication,
    participant: lk.LocalParticipant,
) {

    localVideoFrame.style.display = "none";

    // when local tracks are ended, update UI to remove them from rendering
    publication.track.detach();
    await dotnetObject.invokeMethodAsync("OnLocalTrackUnpublished");
}

function handleActiveSpeakerChange(speakers: lk.Participant[]) {
    // show UI indicators when participant is speaking
}

function handleDisconnect() {
    console.log('disconnected from room');
}

function participantConnected(participant: lk.Participant) {
    console.log("participant", participant.identity, "connected", participant.metadata);
    console.log('tracks', participant.trackPublications);

    participant
        .on(lk.ParticipantEvent.TrackMuted, (pub: lk.TrackPublication) => {
            console.log('track was muted', pub.trackSid, participant.identity);
            displayParticipant(participant);
        })
        .on(lk.ParticipantEvent.TrackUnmuted, (pub: lk.TrackPublication) => {
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

function participantDisconnected(participant: lk.Participant) {
    console.log('participant', participant.identity, 'disconnected');
}

async function handleDevicesChanged() {
    const allDevices = await lk.Room.getLocalDevices(null, false);

    audioInputDevices = allDevices.filter(device => device.kind === 'audioinput');
    audioOutputDevices = allDevices.filter(device => device.kind === 'audiooutput');
    videoInputDevices = allDevices.filter(device => device.kind === 'videoinput');

    allDevices.forEach(x => console.log('device', x.label, x.kind));
}

function displayParticipant(participant: lk.Participant) {

}
