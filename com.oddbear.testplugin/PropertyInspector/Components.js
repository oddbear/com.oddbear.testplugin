// Wrapper helper classes:

// Does sdpi-button make any sense? How should we identify it?

class SdpiBase {
    #sdpi_item;
    #sdpi_component;
    #titleLabel;

    constructor(sdpi_component) {
        /** @type {HTMLElement} */
        this.#sdpi_component = sdpi_component;

        /** @type {HTMLElement} */
        this.#sdpi_item = sdpi_component.parentElement;

        /** @type {HTMLLabelElement} */
        this.#titleLabel = sdpi_component.parentElement.shadowRoot.querySelector('div.label label')
    }

    /** @returns {string} */
    get titleLabel() {
        return this.#titleLabel.textContent;
    }

    /** @param {string} titleLabel */
    set titleLabel(titleLabel) {
        // this.#sdpi_item.setAttribute('label', label); // No need?
        this.#titleLabel.textContent = titleLabel;
    }

    /** @returns {boolean} */
    get hidden() {
        return his.#sdpi_item.style.display == "none";
    }

    /** @param {boolean} hidden */
    set hidden(hidden) {
        this.#sdpi_item.style.display = hidden ? "flex" : "none";
    }

    /**
     * @param {function() : void} handler
     */
    onChange(handler) {
        this.#sdpi_component.addEventListener('valuechange', handler);
    }
}

class SdpiCheckbox extends SdpiBase {
    #sdpi_checkbox;
    #input_checkbox;
    #checkboxLabel;

    constructor(sdpi_checkbox) {
        super(sdpi_checkbox);

        /** @type {Element} */
        this.#sdpi_checkbox = sdpi_checkbox;

        /** @type {HTMLInputElement} */
        this.#input_checkbox = sdpi_checkbox.shadowRoot.querySelector('input[type="checkbox"]');

        /** @type {HTMLSpanElement} */
        this.#checkboxLabel = sdpi_checkbox.shadowRoot.querySelector('span.checkable-text');
    }

    /** @returns {boolean} */
    get checked() {
        return this.#input_checkbox.checked;
    }

    /** @param {boolean} checked */
    set checked(checked) {
        this.#input_checkbox.checked = checked;
    }

    /** @returns {string} */
    get label() {
        return this.#checkboxLabel.textContent;
    }

    /** @param {string} label */
    set label(label) {
        this.#checkboxLabel.textContent = label;
    }

    /** @returns {boolean} */
    get disabled() {
        return this.#input_checkbox.disabled;
    }

    /** @param {boolean} disabled */
    set disabled(disabled) {
        this.#input_checkbox.disabled = disabled;
    }

    /**
     * @param {string} setting
     * @returns {?SdpiCheckbox}
     */
    static search(setting) {
        const sdpi_checkbox = document.querySelector(`sdpi-checkbox[setting="${setting}"]`);
        return sdpi_checkbox
            ? new SdpiCheckbox(sdpi_checkbox)
            : null;
    }
}
class SdpiSelect extends SdpiBase {
    #sdpi_select;
    #select;

    constructor(sdpi_select) {
        super(sdpi_select);

        /** @type {Element} */
        this.#sdpi_select = sdpi_select;

        /** @type {HTMLSelectElement} */
        this.#select = sdpi_select.shadowRoot.querySelector('select');
    }

    // placeholder? This probarbly needs more sinse it's already rendered.
    // label, this is something else.
    // Needs some examples on show-refresh, datasource etc.

    /** @returns {boolean|number|string} */
    get defaultValue() {
        return this.#sdpi_select.defaultValue;
    }

    /** @param {boolean|number|string} value */
    set defaultValue(value) {
        this.#sdpi_select.defaultValue = value;
    }

    /** @returns {boolean|number|string} */
    get value() {
        return this.#select.value;
    }

    /** @param {boolean|number|string} value */
    set value(value) {
        // Triggers settings change event:
        this.#sdpi_select.value = value;
        // Just for sync:
        this.#select.value = value;
    }

    /** @returns {boolean} */
    get disabled() {
        return this.#select.disabled;
    }

    /** @param {boolean} disabled */
    set disabled(disabled) {
        this.#select.disabled = disabled;
    }

    /**
     * @param {string} setting
     * @returns {?SdpiSelect}
     */
    static search(setting) {
        const sdpi_select = document.querySelector(`sdpi-select[setting="${setting}"]`);
        return sdpi_select
            ? new SdpiSelect(sdpi_select)
            : null;
    }
}

class SdpiRange extends SdpiBase {
    #sdpi_range;
    #input_range;

    constructor(sdpi_range) {
        super(sdpi_range);

        /** @type {Element} */
        this.#sdpi_range = sdpi_range;

        /** @type {HTMLInputElement} */
        this.#input_range = sdpi_range.shadowRoot.querySelector('input[type="range"]');
    }

    /** @returns {number} */
    get defaultValue() {
        return this.#sdpi_range.defaultValue;
    }

    /** @param {number} value */
    set defaultValue(value) {
        this.#sdpi_range.defaultValue = value;
    }

    /** @returns {number} */
    get value() {
        return this.#input_range.value;
    }

    /** @param {number} value */
    set value(value) {
        if (this.min > value) {
            value = this.min;
        }

        if (this.max < value) {
            value = this.max;
        }

        // Triggers settings change event:
        this.#sdpi_range.value = value;
        // Just for sync:
        this.#input_range.value = value;
    }


    /** @returns {?number} */
    get min() {
        return this.#sdpi_range.min;
    }

    /** @param {number} min */
    set min(min) {
        this.#sdpi_range.min = min;

        // Search for the slot element with the min value:
        const slot = this.#sdpi_range.querySelector('span[slot="min"]');
        if (slot) {
            slot.textContent = min;
        }

        // Update value if min is out of range:
        if (this.value < min) {
            this.value = min;
        }
    }


    /** @returns {?number} */
    get max() {
        return this.#sdpi_range.max;
    }

    /** @param {number} max */
    set max(max) {
        this.#sdpi_range.max = max;

        // Search for the slot element with the max value:
        const slot = this.#sdpi_range.querySelector('span[slot="max"]');
        if (slot) {
            slot.textContent = max;
        }

        // Update value if max is out of range:
        if (this.value > max) {
            this.value = max;
        }
    }

    /** @returns {boolean} */
    get disabled() {
        return this.#input_range.disabled;
    }

    /** @param {boolean} disabled */
    set disabled(disabled) {
        this.#input_range.disabled = disabled;
    }

    /**
     * @param {string} setting
     * @returns {?SdpiRange}
     */
    static search(setting) {
        const sdpi_range = document.querySelector(`sdpi-range[setting="${setting}"]`);
        return sdpi_range
            ? new SdpiRange(sdpi_range)
            : null;
    }
}