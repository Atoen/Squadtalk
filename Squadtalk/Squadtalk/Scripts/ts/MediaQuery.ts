interface DotnetObject {
    invokeMethodAsync(identifier: string, ...args: any): Promise<void>
    invokeMethod(identifier: string, ...args: any): void
}

const query = window.matchMedia('(min-width: 641px)');
let dotnetObject: DotnetObject;

export function Init(object: DotnetObject) {
    dotnetObject = object;

    query.addEventListener("change",async (e) =>
        dotnetObject.invokeMethodAsync('MediaMatchesChangedCallback', e.matches));

    dotnetObject.invokeMethodAsync('MediaMatchesChangedCallback', query.matches);
}
