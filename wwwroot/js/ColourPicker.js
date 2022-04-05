var colorPicker;

export function InitializeColourPicker(colourSelectedCallback) {
    colorPicker = new iro.ColorPicker("#picker", {
        layout: [{
            component: iro.ui.Wheel,
            options: {}
        }],
        width: 500
    });

    colorPicker.on("input:change", (colorData) => {
        const hue = colorData["$"];
        if (!hue) {
            return;
        }

        const h = hue.h || 0;
        const s = hue.s || 100;
        const l = hue.l || 50;

        colourSelectedCallback.invokeMethodAsync("OnSelectedColour", h, s, l);
    });
}

// TODO: select the colour when a player selects a colour
export function SetColour(h, s, l) {
    colorPicker.color.hsl = {h, s, l};
}