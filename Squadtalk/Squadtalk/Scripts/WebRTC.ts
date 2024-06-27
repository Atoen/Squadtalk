// noinspection JSUnusedGlobalSymbols
// @ts-nocheck

interface DotnetObject {
    invokeMethodAsync(identifier: string, ...args: any): Promise<void>
    invokeMethod(identifier: string, ...args: any): void
}

// @ts-ignore
const room = new LivekitClient.Room({
    adaptiveStream: true,
    dynacast: true,
})

const url = "ws://127.0.0.1:1230/jajo";

let token: string;

room
    .on(LivekitClient.RoomEvent.TrackSubscribed, handleTrackSubscribed)
    .on(LivekitClient.RoomEvent.TrackUnsubscribed, handleTrackUnsubscribed)
    .on(LivekitClient.RoomEvent.ActiveSpeakersChanged, handleActiveSpeakerChange)
    .on(LivekitClient.RoomEvent.Disconnected, handleDisconnect)
    .on(LivekitClient.RoomEvent.LocalTrackUnpublished, handleLocalTrackUnpublished);

let dotnetObject: DotnetObject
let mediaStream: MediaStream
let videoElement: HTMLVideoElement
let videoFrame: HTMLElement

export async function Init(object: DotnetObject) {
    if (!object) {
        throw new Error("dotnet object is undefined")
    }

    dotnetObject = object;
    videoFrame = document.getElementById("video-container");
    videoElement = document.getElementById("local-video") as HTMLVideoElement;
}

function handleTrackSubscribed(
    track: RemoteTrack,
    publication: RemoteTrackPublication,
    participant: RemoteParticipant,
) {
    if (track.kind === Track.Kind.Video || track.kind === Track.Kind.Audio) {
        // attach it to a new HTMLVideoElement or HTMLAudioElement
        const element = track.attach();
        videoFrame.appendChild(element);
    }
}

function handleTrackUnsubscribed(
    track: RemoteTrack,
    publication: RemoteTrackPublication,
    participant: RemoteParticipant,
) {
    // remove tracks from all attached elements
    track.detach();
}

function handleLocalTrackUnpublished(
    publication: LocalTrackPublication,
    participant: LocalParticipant,
) {

    publication.track.detach();
}

function handleActiveSpeakerChange(speakers: Participant[]) {

}

function handleDisconnect() {
    console.log('disconnected from room');
}

export async function Start(token: string): Promise<boolean> {


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
