document.addEventListener("DOMContentLoaded", () => {
    const loginForm = document.getElementById('login-form');
    const errorMsg = document.getElementById('error-msg');

    if (loginForm) {
        loginForm.addEventListener('submit', (event) => {
            event.preventDefault();
            
            const username = document.getElementById('username').value;
            const password = document.getElementById('password').value;
            
            if (username === 'admin' && password === 'pwd') {
                window.location.href = 'admin-dashboard.html';
            } else if (username === 'emp' && password === 'pwd') {
                window.location.href = 'employee-dashboard.html';
            } else {
                errorMsg.textContent = 'Invalid username or password';
            }
        });
    }
});
