let uploadInfo, progress, textBox, filePicker,
    overlay, uploadCancel, upload, interval = null;


export function initialize(instance) {

    uploadInfo = document.getElementById("uploadinfo");
    uploadCancel = document.getElementById("uploadcancel");
    progress = document.getElementById("progressbar");
    textBox = document.getElementById("textBox");
    filePicker = document.getElementById("filePicker");
    overlay = document.getElementById("overlay");

    filePicker.addEventListener("change", async function (e) {
        const file = e.target.files[0];
        await uploadFile(file, instance);
    });

    textBox.addEventListener("paste", async function (e) {
        const items = e.clipboardData.items;
        if (!items) return;

        const item = items[0];
        if (item.type.indexOf("image") === -1) return;

        const blob = item.getAsFile();
        if (blob) {
            const file = new File([blob], "image.png", { type: blob.type });
            await uploadFile(file, instance);
        }
    });

    window.addEventListener("dragenter", function(e) {
        overlay.style.display = "block";
    });

    overlay.addEventListener("dragenter", allowDrag);
    overlay.addEventListener("dragover", allowDrag);

    overlay.addEventListener("dragleave", function(e) {
        overlay.style.display = "none";
    });

    overlay.addEventListener("drop", async function(e) {
        e.preventDefault();
        overlay.style.display = "none";

        const items = e.dataTransfer.items;
        if (!items) return;

        const item = items[0];

        const blob = item.getAsFile();
        if (blob) {
            const file = new File([blob], blob.name, { type: blob.type });
            await uploadFile(file, instance);
        }
    });

    uploadCancel.addEventListener("click", function() {
        if (upload) {
            upload.abort();
            uploadInfo.style.display = "none";
            clearInterval(interval);
        }
    });
}

function allowDrag(e) {
    e.dataTransfer.dropEffect = "copy";
    e.preventDefault();
}

async function uploadFile(file, instance) {

    const chunk = 25 * 1024 * 1024;
    const jwt = await instance.invokeMethodAsync("GetJwt");

    const uploadOptions = {
        endpoint: "http://squadtalk.ddns.net/tus",
        retryDelays: [0, 3000, 5000, 10000, 20000],
        metadata: {
            filename: file.name,
            filetype: file.type,
            filesize: file.size
        },
        chunkSize: chunk,
        headers: { Authorization: `Bearer ${jwt}` },
        onProgress: function(bytesUploaded, bytesTotal) {
            const percentage = bytesUploaded / bytesTotal;
            progress.value = percentage;

            calculateUploadSpeed(bytesUploaded);
        },
        onSuccess: function() {
            uploadInfo.style.display = "none";
            clearInterval(interval);
            console.log("File uploaded");

        },
        onError: function(error) {
            uploadInfo.style.display = "none";
            clearInterval(interval);
            console.log(`Error: ${error}`);
        }
    };

    const isImage = file.type.startsWith("image/");

    uploadInfo.style.display = "grid";
    await instance.invokeMethodAsync("UploadStarted", file.name, file.size.toString());

    if (isImage) {
        const image = new Image();
        image.src = URL.createObjectURL(file);
        image.onload = function () {
            uploadOptions.metadata.width = image.width;
            uploadOptions.metadata.height = image.height;
            upload = new tus.Upload(file, uploadOptions);
            startUpload(upload, instance);
        };
    } else {
        upload = new tus.Upload(file, uploadOptions);
        startUpload(upload, instance);
    }
}

function startUpload(instance) {
    interval = setInterval(async () => {
        const smoothUploadSpeed = calculateSmoothUploadSpeed(uploadSpeedData, 15);
        
        if (instance != null){
            await instance.invokeMethodAsync("UpdateUploadSpeed", Math.floor(smoothUploadSpeed).toString());
        }
    }, 1000);

    upload.findPreviousUploads().then(function (previousUploads) {
        if (previousUploads.length) {
            upload.resumeFromPreviousUpload(previousUploads[0]);
        }

        upload.start();
    });
}

let uploadSpeedData = [];
let totalUploadedBytes = 0;
let lastTimestamp = null;
let updated = false;

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
    }
    updated = false;

    if (data.length > windowSize) {
        data.splice(0, data.length - windowSize);
    }

    // Calculate the sum of the last N data points
    const sum = data.reduce((acc, val) => acc + val, 0);

    return sum / data.length;
}
