let textBox = null;
let parent = null;

export function initialize() {
    textBox = document.getElementById("textBox");
    parent = textBox.parentNode;
    
    textBox.oninput = () => {
        parent.dataset.replicatedValue = textBox.value;
    }
}

export function getAndClearMessage() {
    
    const message = textBox.value;
    textBox.value = "";
    parent.dataset.replicatedValue = "";
    
    return message;
}