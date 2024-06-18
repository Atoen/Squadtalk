interface DotnetObject {
    invokeMethodAsync(identifier: string, ...args: any): Promise<void>
    invokeMethod(identifier: string, ...args: any): void
}

let dotnetObject: DotnetObject
let mediaStream: MediaStream
let videoElement: HTMLVideoElement
let videoFrame: HTMLElement

export function Init(object: DotnetObject) {
    if (!object) {
        throw new Error("dotnet object is undefined")
    }
    
    dotnetObject = object;
}

export async function Start() {
    const displayMediaOptions = {
        video: {
            displaySurface: "window"
        },
        audio: {
            echoCancellation: true,
            noiseSuppression: true,
            sampleRate: 44100,
            suppressLocalAudioPlayback: true
        },
        surfaceSwitching: "include",
        selfBrowserSurface: "exclude",
        systemAudio: "exclude"
    };

    try {
        mediaStream = await navigator.mediaDevices.getDisplayMedia(displayMediaOptions);
        
        videoFrame = document.getElementById("video-container");
        videoFrame.style.display = "block";
        
        videoElement = document.getElementById("local-video") as HTMLVideoElement;
        videoElement.srcObject = mediaStream;
    
        mediaStream.getTracks().forEach(track => {
            track.onended = async () => {
                console.log("Track ended");
                
                Stop();
                await dotnetObject.invokeMethodAsync("OnShareStopped");
            };
        });
    } catch (e) {
        console.log("Error occured", e);
    }
}

export function Stop() {
    if (mediaStream) {
        let tracks = mediaStream.getTracks();
        tracks.forEach(track => track.stop());
        mediaStream = null;
    }
    
    if (videoElement) {
        videoElement.srcObject = null;
        videoFrame.style.display = "none";
    }
}


