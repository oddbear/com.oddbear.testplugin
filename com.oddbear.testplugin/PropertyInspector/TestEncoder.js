/// <reference path="common.js" />

document.addEventListener('DOMContentLoaded', async function () {
    const { streamDeckClient } = SDPIComponents;

    const testDiv = document.getElementById("testDiv");
    testDiv.textContent = entryString("init"); // JavaScript is running

    // needs async on the function (or use .then()):
    // We can ask for ConnectionInfo, settings, etc:
    const result = await streamDeckClient.getConnectionInfo();
    const controller = result.actionInfo.payload.controller;
    //testDiv.textContent = JSON.stringify(result);
    //testDiv.textContent = result.actionInfo.payload.controller;

    if (controller === "Encoder") {
        setSdpiSettingVisibility("action", false);
        updateSdpiRange("volume", "+1 dB", "+10 dB", 1);
    } else {
        setSdpiSettingVisibility("action", true);
        updateSdpiRange("volume", -10, +10, 2);
    }

    // https://docs.elgato.com/streamdeck/sdk/references/websocket/plugin/

    // We can send Commands to the plugin:
    //streamDeckClient.setSettings({
    //    name: "John Doe",
    //    showName: true,
    //    favColor: "green",
    //});

    // We can also inspect general messages:
    //streamDeckClient.message.subscribe((event) => {
    //    testDiv.textContent = "message";
    //});

    // Plugin can send Commands to the "JavaScript" messages:
    streamDeckClient.sendToPropertyInspector.subscribe((event) => {
        testDiv.textContent = entryString("sendToPropertyInspector");
        
        // What is needed is a way to update the UI, ex. the volume range with new values.
    });
});