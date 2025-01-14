
/**
 * Get all the settings from the Property Inspector, and set the visibility of the settings.
 * @param {string} setting
 * @param {bool} isVisible
 */
function setSdpiSettingVisibility(setting, isVisible) {
    const elements = document.querySelectorAll(`[setting="${setting}"]`);
    elements.forEach(element => {
        // Element should be a spdi-item: TODO: Is this needed, or could I just say any element?
        if (element.tagName.toLowerCase().startsWith('sdpi-')) {
            // Label element, aka. sdpi-item:
            const parentElement = element.parentElement;
            if (parentElement) {
                parentElement.style.display = isVisible ? "flex" : "none";
            }
        }
    });
}

/**
 * Set enable or disable on the true HTML elements of the Sdpi-Elements.
 * @param {string} setting
 * @param {boolean} enable
 */
function setSdpiSettingEnabled(setting, enable) {
    const elements = document.querySelectorAll(`[setting="${setting}"]`);
    elements.forEach(element => {
        switch (element.tagName.toLowerCase()) {
            case "sdpi-select":
                element.shadowRoot.querySelector('select').disabled = !enable;
                break;
            case "sdpi-range":
                element.shadowRoot.querySelector('input[type="range"]').disabled = !enable;
                break
        }
    });
}

/**
 * Update the layout for the Range slider element.
 * @param {string} setting
 * @param {string|number} min
 * @param {string|number} max
 * @param {number} step
 * @param {number} [defaultValue]
 */
function updateSdpiRange(setting, min, max, step, defaultValue) {
    const sdpiRange = document.querySelector(`sdpi-range[setting="${setting}"]`);
    if (sdpiRange) {

        // Set a defaultValue as the default, if nothing is already set:
        if (!sdpiRange.value && typeof defaultValue === 'number') {
            sdpiRange.value = defaultValue;
        }

        // Update the textContent of the min:
        const minSpan = sdpiRange.querySelector('span[slot="min"]');
        if (minSpan) {
            minSpan.textContent = min;
            minSpan.style.whiteSpace = "nowrap";
        }

        // Update the textContent of the max:
        const maxSpan = sdpiRange.querySelector('span[slot="max"]');
        if (maxSpan) {
            maxSpan.textContent = max;
            maxSpan.style.whiteSpace = "nowrap";
        }

        // Update the input range min / max:
        const shadowRoot = sdpiRange.shadowRoot;
        if (shadowRoot) {
            const inputRange = shadowRoot.querySelector('input[type="range"]');
            if (inputRange) {
                inputRange.min = parseNumber(min);
                inputRange.max = parseNumber(max);
                inputRange.step = step;
            }
            else {
                console.error('Input range not found');
            }
        }
        else {
            console.error('shadowRoot not found');
        }
    }
    else {
        console.error('sdpi-range element not found');
    }
}

/**
 * Converts a string to a number or returns the number if it is already a number.
 * @param {string|number} numString
 * @returns number
 */
function parseNumber(numString) {
    // No parsing needed:
    if (typeof numString === "number") {
        return numString;
    }

    // Numbers can be like -100, "+100", "-100", "- 100 dB", "- 100.00 dB", "+ 100,00 dB"
    const strValue = numString.toString();
    const strValueClean = strValue.replace(/[^0-9\.,\-]/g, '');
    return parseFloat(strValueClean);
}

/**
 * Just adds datatime after the string.
 * @param {string} str
 * @returns string
 */
function entryString(str) {
    const dateTime = new Date(Date.now());
    return `${dateTime.toISOString()}: ${str}`;
}