let listbox;

export function initialize(lastIndicator, instance)
{
    listbox = document.getElementById("listbox");

    listbox.addEventListener("scroll", () => {
        scrollPositionFromBottom = markScroll();
    });
    
    const options = {
        root: findClosestScrollContainer(lastIndicator),
        rootMargin: "0px",
        threshold: 0,
    };

    if (isValidTableElement(lastIndicator.parentElement)) {
        lastIndicator.style.display = "table-row";
    }

    const observer = new IntersectionObserver(async (entries) => {
        for (const entry of entries) {
            if (entry.isIntersecting) {
                observer.unobserve(lastIndicator);
                await instance.invokeMethodAsync("LoadMoreItems");
            }
        }
    }, options);

    observer.observe(lastIndicator);

    return {
        dispose: () => Dispose(observer),
        onNewItems: () => {
            observer.unobserve(lastIndicator);
            observer.observe(lastIndicator);
        },
    };
}

function findClosestScrollContainer(element) {
    while (element) {
        const style = getComputedStyle(element);
        if (style.overflowY !== "visible") {
            return element;
        }
        element = element.parentElement;
    }
    return null;
}

function Dispose(observer) {
    observer.disconnect();
}

function isValidTableElement(element) {
    if (element === null) {
        return false;
    }
    return ((element instanceof HTMLTableElement && element.style.display === "") || element.style.display === "table")
        || ((element instanceof HTMLTableSectionElement && element.style.display === "") || element.style.display === "table-row-group");
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

let scrollPositionFromBottom = 0;

window.addEventListener("resize", () => {
   scrollToMark(scrollPositionFromBottom);
});
