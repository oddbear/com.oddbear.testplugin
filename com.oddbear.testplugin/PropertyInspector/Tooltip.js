// Must add: <div id="tooltip" class="tooltip"></div> and Tooltip.css

const div_tooltip = document.getElementById('tooltip');
const sdpi_ranges = document.querySelectorAll(`sdpi-range`); // [setting="volume-set"]

sdpi_ranges.forEach(sdpi_range => {
    const input_range = sdpi_range.shadowRoot.querySelector('input[type="range"]');

    // Show the tooltip on mouseover
    input_range.addEventListener('mouseover', function () {
        div_tooltip.style.display = 'block';
        updateTooltip();
    });

    // Hide the tooltip on mouseout
    input_range.addEventListener('mouseout', function () {
        div_tooltip.style.display = 'none';
    });

    // Update the tooltip on input change
    input_range.addEventListener('input', updateTooltip);

    // Function to update the tooltip position and content
    async function updateTooltip() {
        const rect = input_range.getBoundingClientRect();
        const tooltipWidth = div_tooltip.offsetWidth;
        const rangeWidth = rect.width;
        const rangeValue = input_range.value;
        const max = input_range.max;
        const min = input_range.min;

        // Calculate the position of the tooltip
        const left = rect.left + window.scrollX + (rangeWidth * (rangeValue - min) / (max - min)) - (tooltipWidth / 2);
        const top = rect.top + window.scrollY - div_tooltip.offsetHeight;

        // Update tooltip position and content
        div_tooltip.style.left = `${left}px`;
        div_tooltip.style.top = `${top}px`;
        div_tooltip.textContent = `${rangeValue}`;
    }

    // Initial update to ensure correct positioning
    updateTooltip();
});
