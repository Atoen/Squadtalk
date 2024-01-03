"use strict"

let uploadEndpoint = null;

let filePicker = null;
let textBox = null;
let uploadInfo = null;
let progressbar = null;

let selectedFile = null;
let currentUpload = null;

const fileQueue = [];

const { getAssemblyExports } = await globalThis.getDotnetRuntime(0);
const exports = await getAssemblyExports("Squadtalk.Client.dll");

const fileTransfer = exports.Squadtalk.Client.Services.FileTransferService;

export function getMessage() {
    return "Totalne oro";
}

export function removeFromQueue(index) {
    console.log(`Removing ${index} from queue`);
    fileQueue.splice(index, 1);
    
    console.log(`queue length: ${fileQueue.length}`);
}

export function initialize(endpoint) {
    
    uploadEndpoint = endpoint;

    getElements();
    addHandlers();
}

export async function uploadSelectedFile(channelId) {
    if (!selectedFile) {
        console.error("Cannot upload file: no file selected");
        return;
    }

    if (!channelId) {
        console.error("Cannot upload file: invalid channelId");
        return;
    }
    
    const file = selectedFile;
    selectedFile = null;
    
    if (currentUpload) {
        fileQueue.push(file);
        fileTransfer.FileAddedToQueueCallback(file.name, file.size);
        return;
    }
    
    await uploadFile(file, channelId);
}

export function removeSelectedFile() {
    selectedFile = null;
}

export function cancelUpload() {
    if (currentUpload) {
        currentUpload.abort();
    }
    
    uploadEnded(null);
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
    const file = e.target.files[0];
    await selectFile(file);
}

async function handlePaste(e) {
    const items = e.clipboardData.items;
    if (!items || !items.length) return;

    const blob = items[0].getAsFile();
    if (!blob) return;

    const file = new File([blob], blob.name, {type: blob.type});
    await selectFile(file);
}

async function selectFile(file) {
    // if (selectedFile || currentUpload) {
    //     alert("Upload pending...");
    //     return;
    // }

    selectedFile = file;
    fileTransfer.FileSelectedCallback(file.name, file.size);
    // await dotnetInstance.invokeMethodAsync("FileSelectedCallback", file.name, file.size);
}

async function uploadFile(file, channelId) {
    const options = await createUploadOptions(file, channelId);
    currentUpload = new tus.Upload(file, options);
    
    progressbar.style.width = "0";
    uploadInfo.style.display = "grid";

    fileTransfer.UploadStartedCallback(file.name, file.size);

    // currentUpload.start();
}

async function createUploadOptions(file, channelId) {
    const uploadOptions = {
        endpoint: uploadEndpoint,
        retryDelays: [0, 1000, 3000, 5000, 10000],
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
        onError: (error) => {
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

function uploadEnded(error) {
    if (error) {
        console.log(error);
    }

    currentUpload = null;

    fileTransfer.UploadEndedCallback(null);
}

function uploadQueueFinished() {
    uploadInfo.style.display = "none";
}

function loadImage(file){
    return new Promise((resolve) => {
        const image = new Image();
        image.src = URL.createObjectURL(file);
        image.onload = () => resolve(image);
    });
}