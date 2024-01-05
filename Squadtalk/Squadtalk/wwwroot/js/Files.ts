interface DotNetInstance {
    invokeMethodAsync(methodName: string, ...args: any[]): Promise<any>
}

let currentUpload: any;
let dotNetInstance: DotNetInstance;
let uploadEndpoint: string;
let selectedFiles: File[];

let filePicker: HTMLElement;
let textBox: HTMLElement;
let uploadInfo: HTMLElement;
let progressbar: HTMLElement;

export function removeFromQueue(index: number) {
    console.log(`Removing ${index} from queue`);
}

export function initialize(netInstance: DotNetInstance, endpoint: string){
    dotNetInstance = netInstance;
    uploadEndpoint = endpoint

    getElements();
    addHandlers();
}

export async function uploadSelectedFiles(channelId: string) {
    if (!selectedFiles) {
        console.error("Cannot upload files: no file selected");
        return;
    }

    if (!channelId) {
        console.error("Cannot upload file: invalid channelId");
        return;
    }
    
    await uploadFile(selectedFiles[0], channelId);
}

function getElements() {
    filePicker = document.getElementById("file-picker");
    textBox = document.getElementById("textBox");
    uploadInfo = document.getElementById("uploadInfo");
    progressbar = document.getElementById("progressbar");
}

function addHandlers() {
    filePicker.addEventListener("change", HandleFileChange);
    textBox.addEventListener("paste", handlePaste);
}

function HandleFileChange(e: any) {
    const files = e.target.files;
    return  selectFile(files);
}

function handlePaste(e: ClipboardEvent) {
    const items = e.clipboardData.items;
    if (!items || !items.length) return;

    const blob = items[0].getAsFile();
    if (!blob) return;

    const file = new File([blob], blob.name, {type: blob.type});
    return selectFile([file]);
}

function selectFile(files: File[]) {
    selectedFiles = files;
    const first = selectedFiles[0];
    
    return  dotNetInstance.invokeMethodAsync("FileSelectedCallback", first.name, first.size, selectedFiles.length);
}

// @ts-ignore
async function uploadFile(file: File, channelId: string) {
    const options = await createUploadOptions(file, channelId);
    
    // @ts-ignore
    currentUpload = new tus.Upload(file, options);

    progressbar.style.width = "0";
    uploadInfo.style.display = "grid";

    currentUpload.start();

    await dotNetInstance.invokeMethodAsync("UploadStartedCallback", file.name, file.size);
}

async function createUploadOptions(file: File, channelId: string) {
    const uploadOptions = {
        endpoint: "Upload",
        removeFingerprintOnSuccess: true,
        metadata: {
            file_name: file.name,
            content_type: file.type || "application/octet-stream",
            file_size: file.size,
            channel_id: channelId
        },
        chunkSize: 30 * 1000 * 1000,
        onProgress: (bytesUploaded, bytesTotal) => {
            const percentage = bytesUploaded / bytesTotal;
            console.log(percentage);

            progressbar.style.width = percentage * 100 + "%";
        },
        onSuccess: () => {
            uploadEnded(null);
        },
        onError: (error: any) => {
            uploadEnded(error);
        }
    }

    const isImage = file.type.startsWith("image/");
    if (isImage) {
        const image = await loadImage(file);
        uploadOptions.metadata.image_width = image.width;
        uploadOptions.metadata.image_height = image.height;
    }

    return uploadOptions;
}