/// <reference path="common.js" />
/// <reference path="SDPIComponents.js" />
/// <reference path="Components.js" />

document.addEventListener('DOMContentLoaded', async function () {
    const { streamDeckClient } = SDPIComponents;

    const textBoxDiv = document.querySelector('#testTextBox');

    const sdpiSelect = SdpiSelect.search("output");
    const sdpiRange = SdpiRange.search("testValue");
    sdpiSelect.onChange(() => {
        sdpiRange.setValue(-0.5);
        textBoxDiv.textContent = `A: ${sdpiSelect.getValue()}, B: ${sdpiRange.getValue()}`
    });
    sdpiRange.onChange(() => {
        sdpiSelect.setValue("phones");
        textBoxDiv.textContent = `A: ${sdpiSelect.getValue()}, B: ${sdpiRange.getValue()}`
    });
});