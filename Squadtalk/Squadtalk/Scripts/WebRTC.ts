interface DotnetObject {
    invokeMethodAsync(identifier: string, ...args: any): Promise<void>
    invokeMethod(identifier: string, ...args: any): void
}

let dotnetObject: DotnetObject
let mediaStream: MediaStream
let videoElement: HTMLVideoElement
let videoFrame: HTMLElement


// @ts-ignore
console.log(adapter.browserDetails);

export async function Init(object: DotnetObject) {
    if (!object) {
        throw new Error("dotnet object is undefined")
    }
    
    dotnetObject = object;
}

export async function Start(): Promise<boolean> {
    createPeerConnection();
    
    const displayMediaOptions = {
        video: {
            displaySurface: "window"
        },
        audio: {
            echoCancellation: true,
            noiseSuppression: true,
            sampleRate: 44100,
        },
        surfaceSwitching: "include",
        selfBrowserSurface: "exclude"
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
        return false
    }
    
    return true
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

let rtcConnection: RTCPeerConnection;

function createPeerConnection() {
    rtcConnection = new RTCPeerConnection();
    
    rtcConnection.onicecandidate = async e => {
        if (e.candidate) {
            await dotnetObject.invokeMethodAsync("OnIceCandidate", e.candidate);
        }
    }
    
    rtcConnection.ontrack = async e => {
        videoElement.srcObject = e.streams[0];
    }
    
    rtcConnection.onnegotiationneeded = async e => {
        const offer = await rtcConnection.createOffer();
        await rtcConnection.setLocalDescription(offer);
        await dotnetObject.invokeMethodAsync("OnSetLocalDescription", rtcConnection.localDescription);
    }
}
