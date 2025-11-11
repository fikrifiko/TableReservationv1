// open modal
const btnAdminLogin = document.getElementById("btnAdminLogin");
if (btnAdminLogin) {
    btnAdminLogin.addEventListener("click", function (event) {
        event.preventDefault();
        const modalElement = document.getElementById("adminModal");
        if (!modalElement) return;
        const modal = new bootstrap.Modal(modalElement);
        modal.show();
    });
}

// close modal helpers (no inline handlers)
const adminCancelBtn = document.getElementById("adminCancelBtn");
const modalOverlay = document.getElementById("modalOverlay");
function hideAdminModal() {
    const modalElement = document.getElementById("adminModal");
    if (!modalElement) return;
    const modal = bootstrap.Modal.getInstance(modalElement) || new bootstrap.Modal(modalElement);
    modal.hide();
}
if (adminCancelBtn) adminCancelBtn.addEventListener("click", hideAdminModal);
if (modalOverlay) modalOverlay.addEventListener("click", hideAdminModal);

// submit login
const adminLoginForm = document.getElementById("adminLoginForm");
if (adminLoginForm) {
    adminLoginForm.addEventListener("submit", async function (event) {
        event.preventDefault();
        const usernameInput = document.getElementById("username");
        const passwordInput = document.getElementById("password");
        const username = usernameInput ? usernameInput.value : "";
        const password = passwordInput ? passwordInput.value : "";
        try {
            const loginResponse = await fetch("/api/Admin/Login", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ username, password })
            });
            if (!loginResponse.ok) {
                let message = "Nom d'utilisateur ou mot de passe incorrect.";
                try { const err = await loginResponse.json(); if (err.message) message = err.message; } catch {}
                alert(message);
                return;
            }
            const loginData = await loginResponse.json();
            if (loginData.token) {
                window.location.href = "/AdminView/Dashboard";
            }
        } catch (error) {
            alert("Une erreur est survenue. Veuillez réessayer.");
        }
    });
}




