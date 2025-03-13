document.addEventListener('DOMContentLoaded', function () {
    // Find password field - try different selectors to ensure we find it
    const passwordField = document.querySelector('input[id="Password"]') ||
        document.querySelector('input[name="Password"]') ||
        document.querySelector('input[type="password"]');

    if (passwordField) {
        console.log('Password field found:', passwordField.id || passwordField.name);

        // Create password strength elements
        const strengthMeter = document.createElement('div');
        strengthMeter.className = 'password-strength-meter';

        const strengthBar = document.createElement('div');
        strengthBar.className = 'password-strength-bar';

        const strengthBarInner = document.createElement('div');
        strengthBarInner.className = 'password-strength-bar-inner';

        const requirementsContainer = document.createElement('div');
        requirementsContainer.className = 'password-requirements';

        // Define requirements (removed 8 characters requirement)
        const requirements = [
            { id: 'capital', text: 'At least 1 capital letter', regex: /[A-Z]/ },
            { id: 'number', text: 'At least 1 number', regex: /[0-9]/ },
            { id: 'special', text: 'At least 1 special character', regex: /[^A-Za-z0-9]/ }
        ];

        // Create requirement elements
        requirements.forEach(req => {
            const reqElement = document.createElement('div');
            reqElement.className = 'password-requirement';
            reqElement.id = `req-${req.id}`;
            reqElement.textContent = req.text;
            requirementsContainer.appendChild(reqElement);
        });

        // Assemble the strength meter
        strengthBar.appendChild(strengthBarInner);
        strengthMeter.appendChild(strengthBar);
        strengthMeter.appendChild(requirementsContainer);

        // Insert after the password input
        passwordField.parentNode.insertBefore(strengthMeter, passwordField.nextSibling);

        // Add event listener to check password strength
        passwordField.addEventListener('input', function () {
            const password = this.value;
            let strength = 0;
            let metRequirements = 0;

            // Check each requirement
            requirements.forEach(req => {
                const reqElement = document.getElementById(`req-${req.id}`);
                if (!reqElement) return; // Skip if element not found

                const isMet = req.regex.test(password);

                if (isMet) {
                    reqElement.classList.add('met');
                    metRequirements++;
                } else {
                    reqElement.classList.remove('met');
                }
            });

            // Calculate strength based on met requirements
            strength = metRequirements / requirements.length;

            // Update strength bar
            strengthBarInner.className = 'password-strength-bar-inner';

            if (password.length === 0) {
                strengthBarInner.style.width = '0';
            } else if (strength <= 0.33) {
                strengthBarInner.classList.add('weak');
                strengthBarInner.style.width = '33%';
            } else if (strength <= 0.66) {
                strengthBarInner.classList.add('medium');
                strengthBarInner.style.width = '66%';
            } else {
                strengthBarInner.classList.add('strong');
                strengthBarInner.style.width = '100%';
            }
        });

        // Trigger input event to initialize the strength meter
        const event = new Event('input');
        passwordField.dispatchEvent(event);
    } else {
        console.error('Password field not found');
    }
});