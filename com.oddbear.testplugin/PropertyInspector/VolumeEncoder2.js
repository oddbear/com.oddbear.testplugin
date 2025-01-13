/// <reference path="common.js" />

document.addEventListener('DOMContentLoaded', async function () {
    const { streamDeckClient } = SDPIComponents;

    const infoResult = await streamDeckClient.getConnectionInfo();
    const controller = infoResult.actionInfo.payload.controller;
    const isEncoder = controller === "Encoder";
    const isKeypad = controller === "Keypad";

    const settingsLog = document.querySelector('#settingsLog');

    // When receiving settings FROM plugin SetSettingsAsync:
    streamDeckClient.didReceiveSettings.subscribe((actionInfo) => {
        const settingsJson = JSON.stringify(actionInfo.payload.settings);
        settingsLog.textContent = entryString(settingsJson);
    });

    // When receiving settings on init:
    const settings = infoResult.actionInfo.payload.settings;
    if (settings) {
        settingsLog.textContent = `Init: ${JSON.stringify(settings)}`;
        // Will trigger ReceivedSettings in cs plugin:
        //await streamDeckClient.setSettings(settings);
    }

    // SDPI Select Elements:
    const sdpi_select_output = document.querySelector('sdpi-select[setting="output"]');
    const sdpi_select_action = document.querySelector('sdpi-select[setting="action"]');

    // HTML Select Elements:
    const select_output = sdpi_select_output.shadowRoot.querySelector('select');
    const select_action = sdpi_select_action.shadowRoot.querySelector('select');

    // Adding changed events:
    select_output.addEventListener('change', changeEvent);
    select_action.addEventListener('change', changeEvent);

    // Set initial values:
    if (isKeypad) {
        setSdpiSettingEnabled("action", true);
    }
    else {
        select_action.value = "adjust";
    }

    // Trigger initial change event:
    changeEvent();

    function changeEvent() {
        setSdpiRangeVisibility();
        setSdpiRangeValues();
    }

    function setSdpiRangeVisibility() {
        if (!hasOutputSelected() || !hasActionSelected()) {
            return;
        }

        setSdpiSettingVisibility("volume", isVolumeOutput());
        setSdpiSettingVisibility("blend", isBlendOutput());
    }

    function setSdpiRangeValues() {
        // Keypad can adjust and set:
        if (isKeypad) {

            // Keypad Outputs Set:
            if (isVolumeOutput() && isSetAction()) {
                updateSdpiRange("volume", "-96 dB", "0 dB", 1);
                return;
            }

            // Keypad Outputs Adjust:
            if (isVolumeOutput() && isAdjustAction()) {
                updateSdpiRange("volume", "-25 dB", "25 dB", 1);
                return;
            }

            // Keypad Blend Set:
            if (isBlendOutput() && isSetAction()) {
                updateSdpiRange("blend", "-1", "1", 0.1);
                return;
            }

            // Keypad Blend Adjust:
            if (isBlendOutput() && isAdjustAction()) {
                updateSdpiRange("blend", "-0.2", "0.2", 0.1);
                return;
            }

            return;
        }

        // Encoder can only adjust:
        if (isEncoder) {

            // Encoder Outputs Adjust:
            if (isVolumeOutput() && isAdjustAction()) {
                updateSdpiRange("volume", "1 dB", "25 dB", 1);
                return;
            }

            // Encoder Blend Adjust:
            if (isBlendOutput() && isAdjustAction()) {
                updateSdpiRange("blend", "0.1", "0.2", 0.1); // Broken.
                return;
            }

            return;
        }
    }

    function hasOutputSelected() {
        return isVolumeOutput()
            || isBlendOutput();
    }

    function isVolumeOutput() {
        return select_output.value === "mainOut"
            || select_output.value === "phones";
    }

    function isBlendOutput() {
        return select_output.value === "blend";
    }

    function hasActionSelected() {
        return isSetAction()
            || isAdjustAction();
    }

    function isSetAction() {
        // Encoder is always adjust:
        return isKeypad && select_action.value === "set";
    }

    function isAdjustAction() {
        // Encoder is always adjust:
        return isEncoder || select_action.value === "adjust";
    }
});