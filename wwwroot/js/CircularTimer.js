let element;
let animationFrameId;
let dotnetRef;
let propertyName;

export function start(dotnet, elementId, property) {

    dotnetRef = dotnet;
    element = document.getElementById(elementId);
    propertyName = property;

    animationFrameId = requestAnimationFrame(loop);
}

export function stop() {
    cancelAnimationFrame(animationFrameId);
}

async function loop() {
    const value = await dotnetRef.invokeMethodAsync("GetRefreshData");
    element.setAttribute(propertyName, value);
    animationFrameId = requestAnimationFrame(loop);
}