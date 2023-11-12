let listbox;

export function initialize() {
    listbox = document.getElementById("listbox");
}

export function scrollToBottom() {

    const last = listbox.lastElementChild
    if (!last) return;

    const secondToLast = last.previousElementSibling;
    if (!secondToLast) return;

    const scrollThreshold = last.clientHeight + secondToLast.clientHeight;

    const distanceFromBottom = listbox.scrollHeight - listbox.clientHeight - listbox.scrollTop;

    if (distanceFromBottom < scrollThreshold) {
        listbox.scrollTop = listbox.scrollHeight;
    }
}

export function markScroll() {
    return listbox.scrollHeight - listbox.clientHeight - listbox.scrollTop;
}

export function scrollToMark(scrollPositionFromBottom) {
    listbox.scrollTop = listbox.scrollHeight - scrollPositionFromBottom - listbox.clientHeight;
}