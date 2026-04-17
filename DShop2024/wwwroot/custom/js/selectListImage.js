(function () {
    // All your code stays inside here
    const selectContainer = document.getElementById('employeeSelect');
    const trigger = selectContainer?.querySelector('.select-trigger');

    if (!selectContainer || !trigger) return; // Safety check

    const options = selectContainer.querySelectorAll('.options-list li');
    const hiddenInput = document.getElementById('hidden-input');
    const selectedImg = document.getElementById('selected-img');
    const selectedText = document.getElementById('selected-text');

    // Toggle logic
    const toggleDropdown = () => selectContainer.classList.toggle('open');
    trigger.addEventListener('click', toggleDropdown);

    // Selection logic
    options.forEach(option => {
        option.addEventListener('click', () => {

            const value = option.getAttribute('data-value');
            const imgSrc = option.getAttribute('data-img');
            const text = option.innerText;

            // Update UI
            selectedText.innerText = text;
            selectedImg.src = imgSrc;

            // Update Hidden Input
            hiddenInput.value = value;

            // 2. TRIGGER VALIDATION manually so the red text disappears immediately
            $(hiddenInput).valid();

            // Close menu
            selectContainer.classList.remove('open');

        });
    });

    // Global click to close
    window.addEventListener('click', (e) => {
        if (!selectContainer.contains(e.target)) {
            selectContainer.classList.remove('open');
        }
    });
})();