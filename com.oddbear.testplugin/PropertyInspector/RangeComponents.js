/// <reference path="common.js" />
/// <reference path="SDPIComponents.js" />
/// <reference path="Components.js" />

document.addEventListener('DOMContentLoaded', async function () {
    const { streamDeckClient } = SDPIComponents;

    const textBoxDiv = document.querySelector('#testTextBox');

    const sdpiSelect = SdpiSelect.search("output");
    const sdpiRange = SdpiRange.search("testValue");
    const sdpiCheckbox = SdpiCheckbox.search("is_okay");

    sdpiCheckbox.onChange(() => {
        textBoxDiv.textContent = `A: ${sdpiSelect.value}, B: ${sdpiRange.value}, C: ${sdpiCheckbox.checked}`
    });
    sdpiRange.defaultValue = 1;
    sdpiSelect.onChange(() => {
        textBoxDiv.textContent = `A: ${sdpiSelect.value}, B: ${sdpiRange.value}, C: ${sdpiCheckbox.checked}`
    });
    sdpiRange.onChange(() => {
        textBoxDiv.textContent = `A: ${sdpiSelect.value}, B: ${sdpiRange.value}, C: ${sdpiCheckbox.checked}`
    });
});