// noinspection JSUnusedGlobalSymbols

import lk = require("livekit-client");

const Room = lk.Room;
const RoomEvent = lk.RoomEvent;
const ParticipantEvent = lk.ParticipantEvent;
const Track = lk.Track;
const Source = Track.Source;

interface DotnetObject {
    invokeMethodAsync(identifier: string, ...args: any): Promise<void>
    invokeMethod(identifier: string, ...args: any): void
}

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
    .on(RoomEvent.TrackSubscribed, handleTrackSubscribed)
    .on(RoomEvent.TrackUnsubscribed, handleTrackUnsubscribed)
    .on(RoomEvent.Disconnected, handleDisconnect)
    .on(RoomEvent.LocalTrackPublished, handleLocalTrackPublished)
    .on(RoomEvent.LocalTrackUnpublished, handleLocalTrackUnpublished)
    .on(RoomEvent.ParticipantConnected, participantConnected)
    .on(RoomEvent.ParticipantDisconnected, participantDisconnected)
    .on(RoomEvent.MediaDevicesChanged, handleDevicesChanged)
    .on(RoomEvent.MediaDevicesError, async error => {
        const failure = lk.MediaDeviceFailure.getFailure(error);
        await dotnetObject.invokeMethodAsync("ErrorCallback", "Media device error", failure);
    })
    .on(RoomEvent.TrackMuted, handleTrackMuted)
    .on(RoomEvent.TrackUnmuted, handleTrackUnmuted)

const getParticipantAudioElement = (participant: lk.Participant): HTMLAudioElement => {
    const id = `audio-${participant.sid}`;

    const element = document.getElementById(id) as HTMLAudioElement;
    if (element) return element;

    const newElement = document.createElement('audio');
    newElement.id = id;

    return newElement;
}

const removeParticipantAudioElement = (participant: lk.Participant) => {
    const id = `audio-${participant.sid}`;
    const element = document.getElementById(id);

    if (element) {
        element.parentElement?.removeChild(element);
    }
}

let maximizeVideoFrame: HTMLElement;
let maximizeVideoPlayer: HTMLVideoElement;

let dotnetObject: DotnetObject

let roomToken: string;
let serverAddress: string;

let maximizedTrackSid: string | null;
let maximizedParticipantIdentity: string | null;

let cameraFrontFacing = false;
let bitrateInterval: any;

let microphoneEnabled: boolean;
let cameraEnabled: boolean;
let screenShareEnabled: boolean;

export async function Init(object: DotnetObject, url: string) {
    if (!object) {
        throw new Error("dotnet object is undefined")
    }

    dotnetObject = object;
    serverAddress = url;

    room.prepareConnection(serverAddress);

    GetElements();
}

export function GetElements() {
    maximizeVideoFrame = document.getElementById("maximized-video-container");
    maximizeVideoPlayer = document.getElementById("maximized-video") as HTMLVideoElement;
}

export async function Start2(token: string): Promise<boolean> {
    roomToken = token;

    try {
        await room.connect(serverAddress, roomToken);
    } catch (e) {
        await dotnetObject.invokeMethodAsync("ErrorCallback",
            "Failed to connect to the room",
            "Unable to connect to the room");
        return false;
    }

    displayParticipant(room.localParticipant);

    try {
        await room.localParticipant.setMicrophoneEnabled(true);

        microphoneEnabled = true;

        const microphones = await Room.getLocalDevices("audioinput");
        await dotnetObject.invokeMethodAsync("MicrophonesUpdatedCallback", mapMediaDevices(microphones));

        const cameras = await Room.getLocalDevices("videoinput", false);
        await dotnetObject.invokeMethodAsync("CamerasUpdatedCallback", mapMediaDevices(cameras));
    } catch (e) {
        await dotnetObject.invokeMethodAsync("ErrorCallback",
            "Unable to access the microphone",
            "You need to grant access to the microphone in order to let others hear you");
    }

    bitrateInterval = setInterval(displayBitrate, 1000);

    const participant = room.localParticipant;
    participant
        .on(ParticipantEvent.TrackMuted, (pub: lk.TrackPublication) => displayParticipant(participant))
        .on(ParticipantEvent.TrackUnmuted, (pub: lk.TrackPublication) => displayParticipant(participant))
        .on(ParticipantEvent.IsSpeakingChanged, (isSpeaking: boolean) => displayParticipant(participant))
        .on(ParticipantEvent.ConnectionQualityChanged, (connectionQuality: lk.ConnectionQuality) => {
            updateLocalParticipantState();
            displayParticipant(participant);
        })

    await displayParticipant(room.localParticipant);

    return true;
}

export async function Start(token: string): Promise<boolean> {
    roomToken = token;

    try {
        await room.connect(serverAddress, roomToken);

        displayParticipant(room.localParticipant);

        const publication = await room.localParticipant.setMicrophoneEnabled(true);
        if (!publication) {
            await dotnetObject.invokeMethodAsync("ErrorCallback",
                "Unable to access the microphone",
                "You need to grant access to the microphone in order to let others hear you")
        } else {
            microphoneEnabled = true;

            const microphones = await Room.getLocalDevices("audioinput");
            await dotnetObject.invokeMethodAsync("MicrophonesUpdatedCallback", mapMediaDevices(microphones));

            const cameras = await Room.getLocalDevices("videoinput", false);
            await dotnetObject.invokeMethodAsync("CamerasUpdatedCallback", mapMediaDevices(cameras));
        }

        bitrateInterval = setInterval(displayBitrate, 1000);

        const participant = room.localParticipant;
        participant
            .on(ParticipantEvent.TrackMuted, (pub: lk.TrackPublication) => displayParticipant(participant))
            .on(ParticipantEvent.TrackUnmuted, (pub: lk.TrackPublication) => displayParticipant(participant))
            .on(ParticipantEvent.IsSpeakingChanged, (isSpeaking: boolean) => displayParticipant(participant))
            .on(ParticipantEvent.ConnectionQualityChanged, (connectionQuality: lk.ConnectionQuality) => displayParticipant(participant))

        displayParticipant(room.localParticipant);

        return true;
    } catch (e) {
        console.error(e);
        return false;
    }
}

export async function Stop() {
    if (bitrateInterval) clearInterval(bitrateInterval);
    if (maximizedParticipantIdentity) MinimizeVideo();
    await room.disconnect(true);
}

export async function ToggleMicrophoneEnabled() {
    await room.localParticipant.setMicrophoneEnabled(!microphoneEnabled);
    if (!microphoneEnabled) {
        const microphones = await Room.getLocalDevices("audioinput");
        await dotnetObject.invokeMethodAsync("MicrophonesUpdatedCallback", mapMediaDevices(microphones));
    }

    microphoneEnabled = room.localParticipant.isMicrophoneEnabled;
    return microphoneEnabled;
}

export async function ToggleCameraEnabled() {
    await room.localParticipant.setCameraEnabled(!cameraEnabled);
    if (!cameraEnabled) {
        const cameras = await Room.getLocalDevices("videoinput");
        await dotnetObject.invokeMethodAsync("CamerasUpdatedCallback", mapMediaDevices(cameras));
    }

    cameraEnabled = room.localParticipant.isCameraEnabled;
    return cameraEnabled;
}

export async function ToggleScreenShareEnabled() {
    const result = await room.localParticipant.setScreenShareEnabled(!screenShareEnabled);
    screenShareEnabled = room.localParticipant.isScreenShareEnabled;
    return screenShareEnabled;
}

export function ShowVideo(participantIdentity: string, videoMaximized: number) {
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

    // if (source === Source.Camera) {
    //     attachVideo(track, participant, "camera");
    //     const otherTrack = participant.getTrackPublication(Source.ScreenShare)?.track;
    //     detachVideo(otherTrack, participant, "video");
    // }
    // else if (source === Source.ScreenShare) {
    //     attachVideo(track, participant, "video");
    //     const otherTrack = participant.getTrackPublication(Source.Camera)?.track;
    //     detachVideo(otherTrack, participant, "camera");
    // }
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
    const options: lk.VideoCaptureOptions = {
        resolution: lk.VideoPresets.h720.resolution,
        facingMode: cameraFrontFacing ? "user" : "environment"
    };

    cameraPublication.videoTrack?.restartTrack(options);
}

export function ChangeVolume(participantIdentity: string, volume: number, screenShare?: boolean) {
    const participant = room.getParticipantByIdentity(participantIdentity);
    if (participant instanceof lk.RemoteParticipant) {
        const source = screenShare ? Source.ScreenShareAudio : Source.Microphone;
        participant.setVolume(volume / 100, source);
    }
}

export async function ChangeDevice(kind: number, id: string) {
    const mediaDeviceKind: MediaDeviceKind = kind === 0 ? "audioinput" : "videoinput";
    if (room) {
        await room.switchActiveDevice(mediaDeviceKind, id);
    }
}

const mapMediaDevices = (devices: MediaDeviceInfo[]) => devices.map(x => ({
    label: x.label,
    kind: x.kind,
    id: x.deviceId
}));

const mapParticipant = (participant: lk.Participant) => {

    let totalBitrate = 0;
    // @ts-ignore
    for (const t of participant.trackPublications.values()) {
        if (t.track) {
            totalBitrate += t.track.currentBitrate;
        }
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

function attachVideo(track: lk.Track, participant: lk.Participant, prefix: "camera" | "video" | "maximized-video") {
    if (!track) return;

    const container = document.getElementById(`${prefix}-container-${participant.sid}`);
    const player = document.getElementById(`${prefix}-${participant.sid}`) as HTMLVideoElement;
    container.style.display = "block";
    track.attach(player);
}

function detachVideo(track: lk.Track, participant: lk.Participant, prefix: "camera" | "video" | "maximized-video") {
    if (!track) return;

    const container = document.getElementById(`${prefix}-container-${participant.sid}`);
    const player = document.getElementById(`${prefix}-${participant.sid}`) as HTMLVideoElement;
    container.style.display = "none";
    track.detach(player);
}

function handleTrackSubscribed(track: lk.RemoteTrack, publication: lk.RemoteTrackPublication, participant: lk.RemoteParticipant) {
    const camera = participant.isCameraEnabled;
    const screenShare = participant.isScreenShareEnabled;
    const both = screenShare && camera;

    if (track.source === Source.Microphone) {
        const audioPlayer = getParticipantAudioElement(participant);
        track.attach(audioPlayer);


        // const audioPlayer = document.getElementById(`audio-${participant.sid}`) as HTMLAudioElement;
    }
    else if (track.source === Source.ScreenShareAudio) {
        // const screenAudioPlayer = document.getElementById(`screen-audio-${participant.sid}`) as HTMLAudioElement;
        // track.attach(screenAudioPlayer);
    }
    else if (track.source === Source.Camera && !screenShare || both) {
        // attachVideo(track, participant, "camera");
    }
    else if (track.source === Source.ScreenShare && !camera) {
        // attachVideo(track, participant, "video");
    }

    displayParticipant(participant);
}

function handleTrackUnsubscribed(track: lk.RemoteTrack, publication: lk.RemoteTrackPublication, participant: lk.RemoteParticipant) {
    track.detach();

    if (track.source == Source.Microphone) {
        removeParticipantAudioElement(participant);
    }

    else if (track.source === Source.Camera) {
        // const cameraContainer = document.getElementById(`camera-container-${participant.sid}`);
        // cameraContainer.style.display = "none";
    }
    else if (track.source === Source.ScreenShare) {
        // const videoContainer = document.getElementById(`video-container-${participant.sid}`);
        // videoContainer.style.display = "none";
    }
    if (track.sid === maximizedTrackSid) {
        // maximizeVideoFrame.style.display = "none";
    }

    displayParticipant(participant);
}

function handleLocalTrackPublished(publication: lk.LocalTrackPublication) {
    const track = publication.track;
    const participant = room.localParticipant;

    const camera = participant.isCameraEnabled;
    const screenShare = participant.isScreenShareEnabled;
    const both = screenShare && camera;

    // if (track.source === Source.Camera && !screenShare || both) {
    //     attachVideo(track, participant, "camera");
    // }
    // else if (track.source === Source.ScreenShare && !camera) {
    //     attachVideo(track, participant, "video");
    // }

    displayParticipant(participant);
}

function handleLocalTrackUnpublished(
    publication: lk.LocalTrackPublication,
    participant: lk.LocalParticipant) {

    const track = publication.track;
    track.detach();

    // if (track.source === Source.Camera) {
    //     const cameraContainer = document.getElementById(`camera-container-${participant.sid}`);
    //     cameraContainer.style.display = "none";
    // }
    // else if (track.source === Source.ScreenShare) {
    //     const videoContainer = document.getElementById(`video-container-${participant.sid}`);
    //     videoContainer.style.display = "none";
    // }
    //
    // if (track.sid === maximizedTrackSid) {
    //     maximizeVideoFrame.style.display = "none";
    // }

    displayParticipant(participant);
}

function handleTrackUnmuted(
    publication: lk.TrackPublication,
    participant: lk.Participant) {

    const track = publication.track;
    const camera = participant.isCameraEnabled;
    const screenShare = participant.isScreenShareEnabled;
    const both = screenShare && camera;

    // if (track.source === Source.Camera && !screenShare || both) {
    //     const cameraContainer = document.getElementById(`camera-container-${participant.sid}`);
    //     cameraContainer.style.display = "block";
    // }
    // else if (track.source === Source.ScreenShare && !camera) {
    //     const videoContainer = document.getElementById(`video-container-${participant.sid}`);
    //     videoContainer.style.display = "block";
    // }

    displayParticipant(participant);
}

function handleTrackMuted(
    publication: lk.TrackPublication,
    participant: lk.Participant){

    const track = publication.track;

    // if (track.source === Source.Camera) {
    //     const cameraContainer = document.getElementById(`camera-container-${participant.sid}`);
    //     cameraContainer.style.display = "none";
    // }
    // else if (track.source === Source.ScreenShare) {
    //     const videoContainer = document.getElementById(`video-container-${participant.sid}`);
    //     videoContainer.style.display = "none";
    // }
    //
    // if (track.sid === maximizedTrackSid) {
    //     maximizeVideoFrame.style.display = "none";
    // }

    displayParticipant(participant);
}


function handleDisconnect(reason: lk.DisconnectReason) {
    // console.log('disconnected from room. Reason', reason);
    const channelId = room.name;
    dotnetObject.invokeMethodAsync("DisconnectedCallback", reason, channelId);
}

function participantConnected(participant: lk.Participant) {
    console.log("participant", participant.identity, "connected", participant.metadata);
    console.log('tracks', participant.trackPublications);

    participant
        .on(ParticipantEvent.TrackMuted, (pub: lk.TrackPublication) => displayParticipant(participant))
        .on(ParticipantEvent.TrackUnmuted, (pub: lk.TrackPublication) => displayParticipant(participant))
        .on(ParticipantEvent.IsSpeakingChanged, (isSpeaking: boolean) => displayParticipant(participant))
        .on(ParticipantEvent.ConnectionQualityChanged, (connectionQuality: lk.ConnectionQuality) => displayParticipant(participant))

    const channelId = room.name;
    dotnetObject.invokeMethodAsync("ParticipantConnectedCallback", mapParticipant(participant), channelId)
}

function participantDisconnected(participant: lk.Participant) {
    const channelId = room.name;
    console.log('participant', participant.identity, 'disconnected');
    dotnetObject.invokeMethodAsync("ParticipantDisconnectedCallback", mapParticipant(participant), channelId)
}

async function handleDevicesChanged() {
    const allDevices = await Room.getLocalDevices(null, false);
    allDevices.forEach(x => console.log('device', x.label, x.kind));
}

async function displayParticipant(participant: lk.Participant) {
    const channelId = room.name;
    await dotnetObject.invokeMethodAsync("DisplayParticipantCallback", mapParticipant(participant), channelId);
}

async function updateLocalParticipantState() {
    const participant = room?.localParticipant;
    if (!participant) return;

    const localParticipantState = ({
        MicrophoneOn: participant.isMicrophoneEnabled,
        CameraOn: participant.isCameraEnabled,
        ScreenShareOn: participant.isScreenShareEnabled,
        ConnectionQuality: participant.connectionQuality,
    });

    await dotnetObject.invokeMethodAsync("LocalParticipantStateUpdatedCallback", localParticipantState);
}

async function displayBitrate() {
    if (!room || room.state !== lk.ConnectionState.Connected) {
        return;
    }

    // @ts-ignore
    const participants: lk.Participant[] = [...room.remoteParticipants.values()];
    participants.push(room.localParticipant);

    for (const participant of participants) {
        let totalBitrate = 0;
        // @ts-ignore
        for (const t of participant.trackPublications.values()) {
            if (t.track) {
                totalBitrate += t.track.currentBitrate;
            }
        }

        // if (totalBitrate > 0) {
        //     console.debug(`${participant.identity}: ${Math.round(totalBitrate / 1024).toLocaleString()} kbps`);
        // }
    }
}
