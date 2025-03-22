document.addEventListener('DOMContentLoaded', function () {
    // Get all links with the slide-link class
    const slideLinks = document.querySelectorAll('.slide-link');

    // Get the form wrapper and logo container
    const formWrapper = document.querySelector('.form-wrapper');
    const logoContainer = document.getElementById('logoContainer');

    // Check if we're coming from a transition (using sessionStorage)
    const transitionDirection = sessionStorage.getItem('transitionDirection');
    if (transitionDirection) {
        // Clear the stored direction
        sessionStorage.removeItem('transitionDirection');

        // Apply the opposite animation for entry
        if (transitionDirection === 'left') {
            formWrapper.style.animation = 'none';
            formWrapper.offsetHeight; // Trigger reflow
            formWrapper.style.animation = 'slideIn 0.6s cubic-bezier(0.68, -0.55, 0.265, 1.55) forwards';
        } else {
            formWrapper.style.animation = 'none';
            formWrapper.offsetHeight; // Trigger reflow
            formWrapper.style.animation = 'slideIn 0.6s cubic-bezier(0.68, -0.55, 0.265, 1.55) forwards';
        }
    }

    // Add click event to each link
    slideLinks.forEach(link => {
        link.addEventListener('click', function (e) {
            e.preventDefault();

            // Get the target URL and direction
            const targetUrl = this.getAttribute('href');
            const direction = this.getAttribute('data-direction');

            // Store the direction for the next page
            sessionStorage.setItem('transitionDirection', direction);

            // Apply the exit animation to the form
            if (direction === 'left') {
                formWrapper.classList.remove('slide-in');
                formWrapper.classList.add('slide-out-left');

                // Animate logo if it exists
                if (logoContainer) {
                    logoContainer.classList.add('logo-slide-right');
                }
            } else {
                formWrapper.classList.remove('slide-in');
                formWrapper.classList.add('slide-out-right');

                // Animate logo if it exists
                if (logoContainer) {
                    logoContainer.classList.add('logo-slide-left');
                }
            }

            // Navigate to the new page after the animation
            setTimeout(() => {
                window.location.href = targetUrl;
            }, 600); // This should match the animation duration in CSS
        });
    });

    // Store the current page type for animation purposes
    if (window.location.href.includes('Register')) {
        sessionStorage.setItem('currentPage', 'register');
    } else {
        sessionStorage.setItem('currentPage', 'login');
    }