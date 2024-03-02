// noinspection JSUnusedGlobalSymbols

const mediaRecorders: MediaRecorder[] = [];

let dotnetObject: DotnetObject
let audioChunks: Blob[] = [];

interface DotnetObject {
    invokeMethodAsync(identifier: string, ...args: any): Promise<void>
    invokeMethod(identifier: string, ...args: any): void
}

const time = 200;
let recording: boolean = false;
let recorderIndex = 0;

let SwapRecorders: () => void;

export async function Init(object: DotnetObject) {
    dotnetObject = object;
    
    const mediaDeviceInfos = await navigator.mediaDevices.enumerateDevices();
    const microphones = mediaDeviceInfos.filter(x => x.kind === "audioinput");
    
    if (microphones.length === 0) {
        console.log("No microphones detected");
        return;
    }
    
    const stream = await navigator.mediaDevices.getUserMedia({
        audio: { deviceId: microphones[0].deviceId }
    });
    
    mediaRecorders.push(new MediaRecorder(stream));
    mediaRecorders.push(new MediaRecorder(stream));
    
    mediaRecorders.forEach(recorder => {
       recorder.addEventListener("dataavailable", e => {
          if (e.data.size > 0) {
              audioChunks.push(e.data);
          } 
       });
    })
    
    SwapRecorders = () => {
        if (recording) {
            const nextIndex = GetNewIndex(recorderIndex);
            mediaRecorders[nextIndex].start();
            setTimeout(SwapRecorders, time);
        }

        const currentRecorder = mediaRecorders[recorderIndex];
        recorderIndex = GetNewIndex(recorderIndex);
        
        if (currentRecorder.state === "recording") {
            currentRecorder.stop();
        }
        
        const audioBlob = new Blob(audioChunks);
        audioChunks = [];

        const fileReader = new FileReader();
        fileReader.readAsDataURL(audioBlob);
        fileReader.onloadend = () => {
            const base64String = fileReader.result as string;
            dotnetObject.invokeMethod("MicDataCallback", base64String);
        }
    }
}

const GetNewIndex = (index: number) : number => index === 0 ? 1 : 0;

export function PlayAudio(data: string) {
    const audio = new Audio(data);
    audio.play();
}

let timeout: number;

export function StartRecorder(){
    recording = true;
    const recorder = mediaRecorders[recorderIndex];
    
    timeout = setTimeout(SwapRecorders, time);
    
    recorder.start();
    recorder.stream.getAudioTracks().forEach(track => track.enabled = true);
}

export function StopRecorder(){
    recording = false;
    const recorder = mediaRecorders[recorderIndex];

    if (recorder.state === "recording") {
        recorder.stop();
    }
    
    clearTimeout(timeout);
    recorder.stream.getAudioTracks().forEach(track => track.enabled = false);
}