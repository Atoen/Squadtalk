// noinspection JSUnusedGlobalSymbols

// @ts-nocheck
const lk = LivekitClient;

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
        screenShareEncoding: lk.ScreenSharePresets.h720fps30.encoding
    },
    videoCaptureDefaults: {
        resolution: lk.VideoPresets.h720.resolution
    }
});

room
    .on(RoomEvent.TrackSubscribed, trackSubscribed)
    .on(RoomEvent.TrackUnsubscribed, trackUnsubscribed)
    .on(RoomEvent.Disconnected, disconnected)
    .on(RoomEvent.TrackPublished, trackPublished)
    .on(RoomEvent.TrackUnpublished, trackUnpublished)
    .on(RoomEvent.LocalTrackPublished, localTrackPublished)
    .on(RoomEvent.LocalTrackUnpublished, localTrackUnpublished)
    .on(RoomEvent.ParticipantConnected, participantConnected)
    .on(RoomEvent.ParticipantDisconnected, participantDisconnected)
    .on(RoomEvent.MediaDevicesChanged, mediaDevicesChanged)
    .on(RoomEvent.TrackMuted, trackMuted)
    .on(RoomEvent.TrackUnmuted, trackUnmuted)
    .on(RoomEvent.LocalAudioSilenceDetected, localAudioSilenceDetected)
    .on(RoomEvent.MediaDevicesError, mediaDeviceError);

let dotnetObject: DotnetObject

let serverAddress: string;

let maximizedTrackSid: string | null;
let maximizedParticipantIdentity: string | null;

let cameraFrontFacing = false;

let microphoneEnabled: boolean;
let cameraEnabled: boolean;
let screenShareEnabled: boolean;

export function Init(object: DotnetObject, url: string) {
    if (!object) {
        throw new Error("dotnet object is undefined")
    }

    dotnetObject = object;
    serverAddress = url;

    try {
        room.prepareConnection(serverAddress);
    } catch {
        error("Failed to prepare connection", "Unable to prepare connection to the rtc server")
    }
}

export async function Start(token: string): Promise<boolean> {
    try {
        console.log("Starting");

        await room.connect(serverAddress, token);

        console.log("Connected");

    } catch (e) {
        console.log("error" + e);

        error("Failed to connect to the room", "Unable to connect to the room");
        return false;
    }

    console.log(room.state);

    const localParticipant = room.localParticipant;
    localParticipant
        .on(ParticipantEvent.TrackMuted, updateLocalState)
        .on(ParticipantEvent.TrackUnmuted, updateLocalState)
        .on(ParticipantEvent.IsSpeakingChanged, updateLocalState)
        .on(ParticipantEvent.ConnectionQualityChanged, updateLocalState);

    microphoneEnabled = await enableMicrophoneOnJoin();
    if (!microphoneEnabled) {
        error("Unable to access the microphone",
            "You need to grant access to the microphone in order to let others hear you");
    }

    document.documentElement.style.setProperty("--call-info-spacing", "54px");

    updateLocalState();
    updateJoinedParticipants();

    return true;
}

export async function Stop() {
    if (maximizedParticipantIdentity) MinimizeVideo();
    await room.disconnect(true);
}

export async function ToggleMicrophoneEnabled() {
    await room.localParticipant.setMicrophoneEnabled(!microphoneEnabled);
    microphoneEnabled = room.localParticipant.isMicrophoneEnabled;

    if (microphoneEnabled) {
        const microphones = await Room.getLocalDevices("audioinput");
        dotnetObject.invokeMethod("MicrophonesUpdatedCallback", mapMediaDevices(microphones));
    }

    return microphoneEnabled;
}

export async function ToggleCameraEnabled() {
    await room.localParticipant.setCameraEnabled(!cameraEnabled);
    cameraEnabled = room.localParticipant.isCameraEnabled;

    if (cameraEnabled) {
        const cameras = await Room.getLocalDevices("videoinput");
        dotnetObject.invokeMethod("CamerasUpdatedCallback", mapMediaDevices(cameras));
    }

    return cameraEnabled;
}

export async function ToggleScreenShareEnabled() {
    await room.localParticipant.setScreenShareEnabled(!screenShareEnabled, {
        audio: true,
        systemAudio: "include"
    });
    screenShareEnabled = room.localParticipant.isScreenShareEnabled;
    return screenShareEnabled;
}

export function MaximizeVideo(participantIdentity: string, videoMaximized: number) {
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

    maximizedParticipantIdentity = participantIdentity;
    maximizedTrackSid = track.sid;

    const [container, player] = getMaximizedVideoContainerAndPlayer();
    if (container) container.style.display = "block";
    if (player) track.attach(player);
}

export function MinimizeVideo() {
    const [container, player] = getMaximizedVideoContainerAndPlayer();
    if (container) container.style.display = "none";

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

    publication.track.detach(player);

    maximizedTrackSid = null;
    maximizedParticipantIdentity = null;
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

export function ChangeDevice(kind: number, id: string) {
    const mediaDeviceKind: MediaDeviceKind = kind === 0 ? "audioinput" : "videoinput";
    room?.switchActiveDevice(mediaDeviceKind, id);
}

const getMaximizedVideoContainerAndPlayer = (): [HTMLElement?, HTMLVideoElement?] => {
    const container = document.getElementById("maximized-video-container");
    if (!container) {
        console.error("Maximized video container is missing");
    }

    const player = document.getElementById("maximized-video") as HTMLVideoElement;
    if (!player) {
        console.error("Maximized video player is missing");
    }

    return [container, player];
}

const error = (title: string, message: string) =>
    dotnetObject.invokeMethod("ErrorCallback", title, message);

const updateJoinedParticipants = () => {
    const mapped = Array.from(room.remoteParticipants.values())
        .map(mapParticipant);

    dotnetObject.invokeMethod("ParticipantListReceivedCallback", mapped, room.name);
}

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

    dotnetObject.invokeMethod("LocalParticipantStateUpdatedCallback", state);
    updateParticipant(localParticipant);
}

const mapParticipant = (participant: lk.Participant) => {

    return ({
        Username: participant.name,
        Id: participant.identity,
        Sid: participant.sid,
        IsRemote: !participant.isLocal,
        MicrophoneOn: participant.isMicrophoneEnabled,
        CameraOn: participant.isCameraEnabled,
        ScreenShareOn: participant.isScreenShareEnabled,
        ConnectionQuality: participant.connectionQuality,
        IsSpeaking: participant.isSpeaking
    });
};

const updateParticipant = (participant: lk.Participant) =>
    dotnetObject.invokeMethod("ParticipantUpdatedCallback", mapParticipant(participant), room.name);

const enableMicrophoneOnJoin = async (): Promise<boolean> => {
    try {
        const publication = await room.localParticipant.setMicrophoneEnabled(true);
        if (!publication) return false;

        const microphones = await Room.getLocalDevices("audioinput");
        dotnetObject.invokeMethod("MicrophonesUpdatedCallback", mapMediaDevices(microphones));

        const cameras = await Room.getLocalDevices("videoinput", false);
        dotnetObject.invokeMethod("CamerasUpdatedCallback", mapMediaDevices(cameras));

        return true;
    } catch {
        return false;
    }
}

const mapMediaDevices = (devices: MediaDeviceInfo[]) => devices.map(x => ({
    label: x.label,
    kind: x.kind,
    id: x.deviceId
}));

function trackSubscribed(track: lk.RemoteTrack, publication: lk.RemoteTrackPublication, participant: lk.RemoteParticipant){
    if (track.source === Source.Microphone || track.source === Source.ScreenShareAudio) track.attach();

    updateParticipant(participant);
}

function trackUnsubscribed(track: lk.RemoteTrack, publication: lk.RemoteTrackPublication, participant: lk.RemoteParticipant) {
    if (publication.track?.sid === maximizedTrackSid) MinimizeVideo();

    track.detach();
    updateParticipant(participant);
}

function disconnected(reason: lk.DisconnectReason) {
    document.documentElement.style.setProperty("--call-info-spacing", "0px");
    if (reason) dotnetObject.invokeMethod("DisconnectedCallback", reason);
}

function trackPublished(publication: lk.RemoteTrackPublication, participant: lk.RemoteParticipant) {
    updateParticipant(participant);
}

function trackUnpublished(publication: lk.RemoteTrackPublication, participant: lk.RemoteParticipant) {
    if (publication.track?.sid === maximizedTrackSid) MinimizeVideo();

    publication.track?.detach();
    updateParticipant(participant);
}

function localTrackPublished(publication: lk.LocalTrackPublication) {
    updateLocalState();
}

function localTrackUnpublished(publication: lk.LocalTrackPublication, participant: lk.LocalParticipant) {
    if (publication.track?.sid === maximizedTrackSid) MinimizeVideo();

    publication.track?.detach();
    updateLocalState();
}

function participantConnected(participant: lk.Participant) {
    const update = () => updateParticipant(participant);
    participant
        .on(ParticipantEvent.TrackMuted, update)
        .on(ParticipantEvent.TrackUnmuted, update)
        .on(ParticipantEvent.TrackPublished, update)
        .on(ParticipantEvent.TrackUnpublished, update)
        .on(ParticipantEvent.IsSpeakingChanged, update)
        .on(ParticipantEvent.ConnectionQualityChanged, update);

    dotnetObject.invokeMethod("ParticipantConnectedCallback", mapParticipant(participant), room.name);
}

function participantDisconnected(participant: lk.Participant) {
    dotnetObject.invokeMethod("ParticipantDisconnectedCallback", mapParticipant(participant), room.name);
}

function mediaDevicesChanged() {
    Room.getLocalDevices(null, false)
        .then(r =>
            r.forEach(x => console.log('device', x.label, x.kind)));
}

function trackMuted(publication: lk.TrackPublication, participant: lk.Participant) {
    if (publication.track?.sid === maximizedTrackSid) {
        MinimizeVideo()
    }

    if (participant.isLocal) {
        updateLocalState()
    } else {
        updateParticipant(participant);
    }
}

function trackUnmuted(publication: lk.TrackPublication, participant: lk.Participant) {
    if (participant.isLocal) {
        updateLocalState()
    } else {
        updateParticipant(participant);
    }
}

function localAudioSilenceDetected() {

}

function mediaDeviceError(error: Error) {
    const failure = lk.MediaDeviceFailure.getFailure(error);
    dotnetObject.invokeMethod("ErrorCallback", "Media device error", failure);
}
