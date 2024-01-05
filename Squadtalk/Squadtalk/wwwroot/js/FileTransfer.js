// noinspection JSUnusedGlobalSymbols

"use strict"

let uploadEndpoint = null;
let dotNetObject = null;

let filePicker = null;
let textBox = null;
let uploadInfo = null;
let progressbar = null;

let currentUpload = null;

const fileQueue = [];
let selectedFiles = [];

let currentChannelId = "";
let currentChannelName = "";

export function removeFromQueue(index) {
    console.log(`Removing ${index} from queue`);
    fileQueue.splice(index, 1);
    
    console.log(`queue length: ${fileQueue.length}`);
}

export function initialize(object, endpoint) {
    dotNetObject = object;
    uploadEndpoint = endpoint;

    getElements();
    addHandlers();
    
    if (currentUpload) {
        uploadInfo.style.display = "grid";
    }
}

export async function uploadSelectedFiles(channelId, channelName) {
        
    if (!selectedFiles) {
        console.error("Cannot upload file: no file selected");
        return;
    }

    if (!channelId) {
        console.error("Cannot upload file: invalid channelId");
        return;
    }
    
    if (currentUpload || fileQueue.length) {
        if (channelId !== currentChannelId){
            await dotNetObject.invokeMethodAsync(
                "InvalidUploadCallback", 
                `A pending upload is currently active on another channel: ${currentChannelName}`);
            return;
        }
        
        for (let i = 0; i < selectedFiles.length; i++) {
            const file = selectedFiles[i];
            fileQueue.push(file);
            await dotNetObject.invokeMethodAsync("FileAddedToQueueCallback", file.name, file.size);
        }
        
        return;
    }
    
    currentChannelId = channelId;
    currentChannelName = channelName;
    
    const [first, ...rest] = selectedFiles;
    
    for (let i = 0; i < rest.length; i++) {
        const file = rest[i];
        fileQueue.push(file);
        await dotNetObject.invokeMethodAsync("FileAddedToQueueCallback", file.name, file.size);
    }
    
    await uploadFile(first, channelId);
}

export function removeSelectedFile() {
    selectedFiles = [];
}

export async function cancelUpload() {
    if (currentUpload) {
        currentUpload.abort(true);
    }
    
    await uploadEnded(null);
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

async function HandleFileChange(e) {
    const files = e.target.files;
    await selectFile(files);
}

async function handlePaste(e) {
    const items = e.clipboardData.items;
    if (!items || !items.length) return;

    const blob = items[0].getAsFile();
    if (!blob) return;

    const file = new File([blob], blob.name, {type: blob.type});
    await selectFile([file]);
}

async function selectFile(files) {
    selectedFiles = files;
    const first = selectedFiles[0];
    await dotNetObject.invokeMethodAsync("FileSelectedCallback", first.name, first.size, selectedFiles.length);
}

async function uploadFile(file, channelId) {
    const options = await createUploadOptions(file, channelId);
    currentUpload = new tus.Upload(file, options);
    
    progressbar.style.width = "0";
    uploadInfo.style.display = "grid";
    
    await dotNetObject.invokeMethodAsync("UploadStartedCallback", file.name, file.size);

    currentUpload.start();
}

async function createUploadOptions(file, channelId) {
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
        onSuccess: async () => {
            await uploadEnded(null);
        },
        onError: async (error) => {
            await uploadEnded(error);
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

async function uploadNextFileFromQueue() {
    if (!fileQueue.length) {
        uploadInfo.style.display = "none";
        return;
    }
    
    console.log(fileQueue);
    const file = fileQueue.shift();
    console.log(file);
    if (file) {
        await dotNetObject.invokeMethodAsync("UploadingFileFromQueueCallback");
        await uploadFile(file, "global");
    }
}

async function uploadEnded(error) {
    if (error) {
        console.log(error);
    }

    currentUpload = null;
    await uploadNextFileFromQueue();
}

function loadImage(file){
    return new Promise((resolve) => {
        const image = new Image();
        image.src = URL.createObjectURL(file);
        image.onload = () => resolve(image);
    });
}