let listbox;

export function initialize() {
    listbox = document.getElementById("listbox");
}

export function scrollToBottom() {
    
    const last = listbox.lastElementChild
    const scrollThreshold = last.clientHeight * 1.2;
    
    const distanceFromBottom = listbox.scrollHeight - listbox.clientHeight - listbox.scrollTop;
    
    if (distanceFromBottom < scrollThreshold) {
        listbox.scrollTop = listbox.scrollHeight;
    }
}

export function markScroll() {
    return  listbox.scrollHeight - listbox.clientHeight - listbox.scrollTop;
}

export function scrollToMark(scrollPositionFromBottom) {    
    listbox.scrollTop = listbox.scrollHeight - scrollPositionFromBottom - listbox.clientHeight;
}