$(document).ready(function () {
    // Handle login form submission
    $('#loginForm').on('submit', function (e) {
        // Only show loading if form validation passes
        if ($(this).valid()) {
            $('#loginButton .btn-text').hide();
            $('#loadingSpinner').show();
            $('#loginButton').prop('disabled', true);
        }
    });

    // Handle register form submission
    $('#registerForm').on('submit', function (e) {
        // Only show loading if form validation passes
        if ($(this).valid()) {
            $('#registerButton .btn-text').hide();
            $('#loadingSpinner').show();
            $('#registerButton').prop('disabled', true);

        }
    });
});