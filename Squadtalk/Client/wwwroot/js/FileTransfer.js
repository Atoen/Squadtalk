let uploadInfo, progress, textBox, filePicker, overlay, upload, interval = null;
let uploadSpeedData = [];
let instance = null;
let totalUploadedBytes = 0;
let lastTimestamp = null;
let updated = false;
let fileToUpload = null;
let uploadOptions = null;

export function initialize(csInstance) {

    instance = csInstance;

    initializeElements();
    addEventListeners();
}

export function isFileUploadReady() {
    return fileToUpload !== null && !upload;
}

function initializeElements() {
    uploadInfo = document.getElementById("uploadinfo");
    // uploadCancel = document.getElementById("uploadcancel");
    progress = document.getElementById("progressbar");
    textBox = document.getElementById("textBox");
    filePicker = document.getElementById("filePicker");
    overlay = document.getElementById("overlay");
}

function addEventListeners() {
    filePicker.addEventListener("change", handleFileChange);
    textBox.addEventListener("paste", handlePaste);
    window.addEventListener("dragenter", showOverlay);
    overlay.addEventListener("dragenter", allowDrag);
    overlay.addEventListener("dragover", allowDrag);
    overlay.addEventListener("dragleave", hideOverlay);
    overlay.addEventListener("drop", handleDrop);
    // uploadCancel.addEventListener("click", CancelUpload);
}

function allowDrag(e) {
    e.dataTransfer.dropEffect = "copy";
    e.preventDefault();
}

function showOverlay() {
    overlay.style.display = "block";
}

function hideOverlay() {
    overlay.style.display = "none";
}

async function handleFileChange(e) {
    const file = e.target.files[0];
    await selectFile(file);
}

async function selectFile(file) {

    if (fileToUpload) {
        alert("Upload pending...");
        return;
    }

    fileToUpload = file;
    await instance.invokeMethodAsync("FileSelectedCallback", fileToUpload.name, fileToUpload.size.toString());
}

async function handlePaste(e) {
    const items = e.clipboardData.items;
    if (!items) return;

    const item = items[0];
    if (item.type.indexOf("image") === -1) return;

    const blob = item.getAsFile();
    if (blob) {
        const file = new File([blob], "image.png", {type: blob.type});
        await selectFile(file);
    }
}

async function handleDrop(e) {
    e.preventDefault();
    hideOverlay();

    const items = e.dataTransfer.items;
    if (!items) return;

    const item = items[0];

    const blob = item.getAsFile();
    if (blob) {
        const file = new File([blob], blob.name, {type: blob.type});
        await selectFile(file);
    }
}

export function removeSelectedFile() {
    fileToUpload = null;
}

export async function uploadSelectedFile(channelId) {
    await uploadFile(fileToUpload, channelId);
}

export function updateJwt(jwt){
    if (uploadOptions) {
        uploadOptions.headers.Authorization = `Bearer ${jwt}`;
        console.log("auth header updated");
    }
}

async function uploadFile(file, channel) {
    const chunk = 30 * 1000 * 100;
    const jwt = await instance.invokeMethodAsync("GetJwt");

    progress.style.width = "0";

    const uploadOptions = {
        endpoint: "https://squadtalk.net/tus",
        retryDelays: [0, 3000, 5000, 10000, 20000],
        metadata: {
            filename: file.name,
            filetype: file.type,
            filesize: file.size,
            channelId: channel
        },
        chunkSize: chunk,
        headers: {Authorization: `Bearer ${jwt}`},
        onProgress: function (bytesUploaded, bytesTotal) {

            const percentage = bytesUploaded / bytesTotal;
            progress.style.width = percentage * 100 + "%";

            calculateUploadSpeed(bytesUploaded);
        },
        onSuccess: async () => {
            await uploadEnded(null);
        },
        onError: async error => {
            await uploadEnded(error);
        },
        onShouldRetry: async (err, retryAtempt, options) => {
            var status = err.originalResponse ? err.originalResponse.getStatus() : 0;

            if (status === 401) {
                const newJwt = await instance.invokeMethodAsync("GetJwt");
                updateJwt(newJwt);
            }

            return true;
        }
    };

    const isImage = file.type.startsWith("image/");

    await showUploadInfo(file);

    if (isImage) {
        const image = await loadImage(file);
        uploadOptions.metadata.width = image.width;
        uploadOptions.metadata.height = image.height;
    }

    upload = new tus.Upload(file, uploadOptions);
    startUpload(instance);
}

async function loadImage(file) {
    return new Promise((resolve) => {
        const image = new Image();
        image.src = URL.createObjectURL(file);
        image.onload = () => resolve(image);
    });
}

async function showUploadInfo(file) {
    uploadInfo.style.display = "grid";
    await instance.invokeMethodAsync("UploadStartedCallback", file.name, file.size.toString());
}

function startUpload() {
    interval = setInterval(async () => {
        const smoothUploadSpeed = calculateSmoothUploadSpeed(uploadSpeedData, 15);

        if (instance != null) {
            const uploadSpeed = Math.floor(smoothUploadSpeed).toString();

            await instance.invokeMethodAsync("UpdateUploadSpeedCallback", uploadSpeed);
        }
    }, 1000);

    upload.findPreviousUploads().then(previousUploads => {
        if (previousUploads.length) {
            upload.resumeFromPreviousUpload(previousUploads[0]);
        }

        upload.start();
    });
}

export async function CancelUpload() {
    if (upload) {
        upload.abort();
    }

    await uploadEnded(null);
}

async function uploadEnded(reason) {
    upload = null;
    fileToUpload = null;
    uploadInfo.style.display = "none";
    clearInterval(interval);
    await instance.invokeMethodAsync("UploadEndedCallback", reason);
    console.log(reason);
}


function calculateUploadSpeed(uploadedBytes) {
    const currentTime = Date.now();

    let elapsedTime = 0;
    if (lastTimestamp !== null) {
        elapsedTime = currentTime - lastTimestamp;
    }
    lastTimestamp = currentTime;

    if (elapsedTime === 0) {
        return;
    }

    updated = true;

    const currentUploadSpeed = (uploadedBytes - totalUploadedBytes) / (elapsedTime / 1000);
    totalUploadedBytes = uploadedBytes;

    uploadSpeedData.push(currentUploadSpeed);
}

function calculateSmoothUploadSpeed(data, windowSize) {
    if (data.length === 0) {
        return 0;
    }

    if (!updated) {
        uploadSpeedData.push(0);
        uploadSpeedData.push(0);
    }
    updated = false;

    if (data.length > windowSize) {
        data.splice(0, data.length - windowSize);
    }

    const sum = data.reduce((acc, val) => acc + val, 0);

    return sum / data.length;
}