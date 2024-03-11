// noinspection JSUnusedGlobalSymbols
// @ts-ignore
Howler.autoUnlock = false;
const mediaRecorders = [];
let recorder;
let dotnetObject;
let audioChunks = [];
const time = 200;
let recording = false;
let recorderIndex = 0;
let SwapRecorders;
export async function Init(object) {
    dotnetObject = object;
    const mediaDeviceInfos = await navigator.mediaDevices.enumerateDevices();
    const microphones = mediaDeviceInfos.filter(x => x.kind === "audioinput");
    if (microphones.length === 0) {
        console.log("No microphones detected");
        return;
    }
    const stream = await navigator.mediaDevices.getUserMedia({
        audio: { deviceId: microphones[0].deviceId, echoCancellation: true }
    });
    const options = { mimeType: "audio/webm" };
    mediaRecorders.push(new MediaRecorder(stream, options));
    mediaRecorders.push(new MediaRecorder(stream, options));
    mediaRecorders.forEach(recorder => {
        recorder.addEventListener("dataavailable", e => {
            if (e.data.size > 0) {
                audioChunks.push(e.data);
            }
        });
    });
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
            const base64String = fileReader.result;
            dotnetObject.invokeMethod("MicDataCallback", base64String);
        };
    };
}
const GetNewIndex = (index) => index === 0 ? 1 : 0;
export function PlayAudio(data) {
    try {
        // const audio = new Audio(data);
        // await audio.play();
        // @ts-ignore
        const sound = new Howl({
            src: [data],
            format: ["webm"]
        });
        sound.play();
    }
    catch (e) {
        console.log(e);
    }
}
let timeout;
export function StartRecorder() {
    recording = true;
    const recorder = mediaRecorders[recorderIndex];
    timeout = setTimeout(SwapRecorders, time);
    recorder.start();
    recorder.stream.getAudioTracks().forEach(track => track.enabled = true);
}
export function StopRecorder() {
    recording = false;
    const recorder = mediaRecorders[recorderIndex];
    if (recorder.state === "recording") {
        recorder.stop();
    }
    clearTimeout(timeout);
    recorder.stream.getAudioTracks().forEach(track => track.enabled = false);
}
//# sourceMappingURL=VoiceChat.js.map