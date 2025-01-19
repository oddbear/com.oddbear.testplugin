// Wrapper helper classes:
class SdpiBase {
    #sdpi_item;

    constructor(sdpi_item) {
        /** @type {?HTMLElement} */
        this.#sdpi_item = sdpi_item;
    }

    setVisibility(isVisible) {
        this.#sdpi_item.style.display = isVisible ? "flex" : "none";
    }
}

class SdpiSelect extends SdpiBase {
    #sdpi_select;
    #select;

    constructor(sdpi_item, sdpi_select, select) {
        super(sdpi_item);

        /** @type {?Element} */
        this.#sdpi_select = sdpi_select;

        /** @type {?HTMLSelectElement} */
        this.#select = select;
    }

    /**
     * @returns {number}
     */
    getValue() {
        return this.#select.value;
    }

    setValue(value) {
        // Triggers settings change event:
        this.#sdpi_select.value = value;
        // Just for sync:
        this.#select.value = value;
    }

    /**
     * @param {boolean} isEnable
     */
    setEnabled(isEnable) {
        this.#select.disabled = !isEnable;
    }

    /**
     * @param {function() : void} handler
     */
    onChange(handler) {
        this.#select.addEventListener('change', handler);
    }

    /**
     * @param {string} setting
     * @returns {?SdpiSelect}
     */
    static search(setting) {
        const sdpi_select = document.querySelector(`sdpi-select[setting="${setting}"]`);
        if (!sdpi_select) {
            return null;
        }

        const sdpi_item = sdpi_select.parentElement;
        const select = sdpi_select.shadowRoot.querySelector('select');
        return new SdpiSelect(sdpi_item, sdpi_select, select);
    }
}

class SdpiRange extends SdpiBase {
    #sdpi_range;
    #input_range;

    constructor(sdpi_item, sdpi_range, input_range) {
        super(sdpi_item);

        /** @type {?Element} */
        this.#sdpi_range = sdpi_range;

        /** @type {?HTMLInputElement} */
        this.#input_range = input_range;
    }

    /**
     * @returns {number}
     */
    getValue() {
        return this.#input_range.value;
    }

    /**
     * @param {number} value
     */
    setValue(value) {
        // Triggers settings change event:
        this.#sdpi_range.value = value;
        // Just for sync:
        this.#input_range.value = value;
    }

    /**
     * @param {boolean} isEnable
     */
    setEnabled(isEnable) {
        this.#input_range.disabled = !isEnable;
    }

    /**
     * @param {function() : void} handler
     */
    onChange(handler) {
        this.#input_range.addEventListener('change', handler);
    }

    /**
     * @param {string} setting
     * @returns {?SdpiRange}
     */
    static search(setting) {
        const sdpi_range = document.querySelector(`sdpi-range[setting="${setting}"]`);
        if (!sdpi_range) {
            return null;
        }

        const sdpi_item = sdpi_range.parentElement;
        const input_range = sdpi_range.shadowRoot.querySelector('input[type="range"]');
        return new SdpiRange(sdpi_item, sdpi_range, input_range);
    }
}