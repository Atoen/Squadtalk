let dotnetObject: DotnetObject

interface DotnetObject {
    invokeMethodAsync(identifier: string, ...args: any): Promise<void>
}

export async function Init(object: DotnetObject, bitrate: number) {
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

    const options: MediaRecorderOptions = {mimeType: 'audio/webm', audioBitsPerSecond: bitrate};
    const mediaRecorder = new MediaRecorder(stream, options);

    mediaRecorder.addEventListener('dataavailable', async e => {
        if (e.data.size > 0) {
            const data = await e.data.text();
            await dotnetObject.invokeMethodAsync("MicDataCallback", data);
        } 
    });
    
    mediaRecorder.start(100);
}