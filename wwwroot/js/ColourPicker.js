var colorPicker;

export function InitializeColourPicker() {
    colorPicker = new iro.ColorPicker("#picker", {
        layout: [{
                component: iro.ui.Wheel,
                options: {}
        }],
        width: 500
    });
}