// noinspection JSUnusedGlobalSymbols

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

let roomToken: string;

let dotnetObject: DotnetObject
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

export async function Start(token: string): Promise<boolean> {
    roomToken = token;

    try {
        await room.prepareConnection(url, roomToken);
        await room.connect('ws://127.0.0.1:1230/jajo', roomToken);

        const participant = room.localParticipant;
        await participant.setScreenShareEnabled(true);

        return true;
    } catch {
        return false;
    }
}

export async function Stop() {
    const participant = room?.localParticipant;
    if (!participant) return;

    await participant.setScreenShareEnabled(false);
}
