document.addEventListener('DOMContentLoaded', async function () {
    const { streamDeckClient } = SDPIComponents;

    const infoResult = await streamDeckClient.getConnectionInfo();
    const controller = infoResult.actionInfo.payload.controller;
    const isEncoder = controller === "Encoder";
    const isKeypad = controller === "Keypad";

    // SDPI Select Elements:
    const sdpi_select_output = document.querySelector('sdpi-select[setting="output"]');
    const sdpi_select_action_volume = document.querySelector('sdpi-select[setting="volume-action"]');
    const sdpi_select_action_blend = document.querySelector('sdpi-select[setting="blend-action"]');

    // HTML Select Elements:
    const select_output = sdpi_select_output.shadowRoot.querySelector('select');
    const select_action_volume = sdpi_select_action_volume.shadowRoot.querySelector('select');
    const select_action_blend = sdpi_select_action_blend.shadowRoot.querySelector('select');

    // Adding changed events:
    select_output.addEventListener('change', outputChanged);
    select_action_volume.addEventListener('change', actionChanged);
    select_action_blend.addEventListener('change', actionChanged);

    // Might be refresh of PropertyInspector:
    outputChanged();

    function outputChanged() {
        if (!hasOutputSelected()) {
            // No Output, so no state:
            return;
        }

        // We know we have a valid Output value here:
        setActionVisibility();

        // We know we have a valid State here:
        setSdpiRangeValues();
        setSdpiRangeVisibility();
    }

    function actionChanged() {
        if (!hasActionSelected()) {
            // No Action, so no valid state:
            return;
        }

        // We know we have a valid State here:
        setSdpiRangeValues();
        setSdpiRangeVisibility();
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
        if (isEncoder) {
            return false;
        }

        if (isVolumeOutput()) {
            return select_action_volume.value === "set";
        }

        if (isBlendOutput()) {
            return select_action_blend.value === "set";
        }

        return false;
    }

    function isAdjustAction() {
        // Encoder is always adjust:
        if (isEncoder) {
            return true;
        }

        if (isVolumeOutput()) {
            return select_action_volume.value === "adjust";
        }

        if (isBlendOutput()) {
            return select_action_blend.value === "adjust";
        }

        return false;
    }

    function setActionVisibility() {
        if (isKeypad && isVolumeOutput()) {
            setSdpiSettingVisibility("volume-action", true);
            setSdpiSettingVisibility("blend-action", false);
        }
        else if (isKeypad && isBlendOutput()) {
            setSdpiSettingVisibility("volume-action", false);
            setSdpiSettingVisibility("blend-action", true);
        }
        else {
            setSdpiSettingVisibility("volume-action", false);
            setSdpiSettingVisibility("blend-action", false);
        }
    }

    function setSdpiRangeVisibility() {
        if (isVolumeOutput() && isSetAction()) {
            setSdpiSettingVisibility("volume-set", true);
            setSdpiSettingVisibility("volume-adjust", false);
            setSdpiSettingVisibility("blend-set", false);
            setSdpiSettingVisibility("blend-adjust", false);
        }
        else if (isVolumeOutput() && isAdjustAction()) {
            setSdpiSettingVisibility("volume-set", false);
            setSdpiSettingVisibility("volume-adjust", true);
            setSdpiSettingVisibility("blend-set", false);
            setSdpiSettingVisibility("blend-adjust", false);
        }
        else if (isBlendOutput() && isSetAction()) {
            setSdpiSettingVisibility("volume-set", false);
            setSdpiSettingVisibility("volume-adjust", false);
            setSdpiSettingVisibility("blend-set", true);
            setSdpiSettingVisibility("blend-adjust", false);
        }
        else if (isBlendOutput() && isAdjustAction()) {
            setSdpiSettingVisibility("volume-set", false);
            setSdpiSettingVisibility("volume-adjust", false);
            setSdpiSettingVisibility("blend-set", false);
            setSdpiSettingVisibility("blend-adjust", true);
        }
        else {
            setSdpiSettingVisibility("volume-set", false);
            setSdpiSettingVisibility("volume-adjust", false);
            setSdpiSettingVisibility("blend-set", false);
            setSdpiSettingVisibility("blend-adjust", false);
        }
    }

    function setSdpiRangeValues() {
        if (isEncoder && isVolumeOutput()) {
            // Encoder Outputs Adjust:
            updateSdpiRange("volume-adjust", "1 dB", "25 dB", 1);
        }
        else if (isEncoder && isBlendOutput()) {
            // Encoder Blend Adjust:
            updateSdpiRange("volume-adjust", "0.1", "0.2", 0.1);
        }
        else if (isKeypad && isVolumeOutput()) {
            // Keypad Outputs Set:
            updateSdpiRange("volume-set", "-96 dB", "0 dB", 1);
            // Keypad Outputs Adjust:
            updateSdpiRange("volume-adjust", "-25 dB", "25 dB", 1);
        }
        else if (isKeypad && isBlendOutput()) {
            // Keypad Blend Set:
            updateSdpiRange("volume-set", "-1", "1", 0.1);
            // Keypad Blend Adjust:
            updateSdpiRange("volume-adjust", "-0.2", "0.2", 0.1);
        }
    }
});