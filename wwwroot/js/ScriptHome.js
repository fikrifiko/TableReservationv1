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

// client login modal handling
const btnClientLogin = document.getElementById("btnClientLogin");
const clientLoginModalElement = document.getElementById("clientLoginModal");
const clientLoginError = document.getElementById("clientLoginError");

function showClientLoginModal() {
    if (!clientLoginModalElement) return;
    const modal = bootstrap.Modal.getOrCreateInstance(clientLoginModalElement);
    if (clientLoginError) {
        clientLoginError.classList.add("d-none");
        clientLoginError.textContent = "";
    }
    modal.show();
}

if (btnClientLogin) {
    btnClientLogin.addEventListener("click", event => {
        event.preventDefault();
        showClientLoginModal();
    });
}

if (clientLoginModalElement) {
    clientLoginModalElement.addEventListener("hidden.bs.modal", () => {
        if (clientLoginError) {
            clientLoginError.classList.add("d-none");
            clientLoginError.textContent = "";
        }
        const form = document.getElementById("clientLoginForm");
        if (form) form.reset();
    });
}

const clientLoginForm = document.getElementById("clientLoginForm");
if (clientLoginForm) {
    clientLoginForm.addEventListener("submit", async event => {
        event.preventDefault();
        const emailInput = document.getElementById("clientLoginEmail");
        const passwordInput = document.getElementById("clientLoginPassword");
        const email = emailInput ? emailInput.value.trim() : "";
        const password = passwordInput ? passwordInput.value.trim() : "";
        const errorRequired = clientLoginForm.dataset.errorRequired || "Veuillez remplir tous les champs.";
        const errorCredentials = clientLoginForm.dataset.errorCredentials || "Email ou mot de passe incorrect.";
        const errorGeneric = clientLoginForm.dataset.errorGeneric || "Une erreur est survenue. Veuillez réessayer.";

        if (!email || !password) {
            if (clientLoginError) {
                clientLoginError.textContent = errorRequired;
                clientLoginError.classList.remove("d-none");
            }
            return;
        }

        try {
            const response = await fetch("/api/client/login", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ email, password })
            });

            if (!response.ok) {
                let message = errorCredentials;
                try {
                    const errorData = await response.json();
                    if (errorData.message) {
                        message = errorData.message;
                    }
                } catch (err) {
                    console.warn("Unable to parse client login error response", err);
                }
                if (clientLoginError) {
                    clientLoginError.textContent = message;
                    clientLoginError.classList.remove("d-none");
                }
                return;
            }

            window.location.href = "/ClientAccount/Reservations";
        } catch (error) {
            if (clientLoginError) {
                clientLoginError.textContent = errorGeneric;
                clientLoginError.classList.remove("d-none");
            }
        }
    });
}

const clientLogoutBtn = document.getElementById("clientLogoutBtn");
if (clientLogoutBtn) {
    clientLogoutBtn.addEventListener("click", async () => {
        try {
            const response = await fetch("/api/client/logout", { method: "POST" });
            if (response.ok) {
                window.location.href = "/";
            }
        } catch (error) {
            console.error("Erreur lors de la déconnexion client", error);
        }
    });
}




