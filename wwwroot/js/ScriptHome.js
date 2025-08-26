// open modal
const btnAdminLogin = document.getElementById("btnAdminLogin");
if (btnAdminLogin) {
    btnAdminLogin.addEventListener("click", function (event) {
        event.preventDefault();
        let modalElement = document.getElementById("adminModal");
        let modal = new bootstrap.Modal(modalElement);
        modal.show();
    });
}

// close modal
const adminCancelBtn = document.getElementById("adminCancelBtn");
const modalOverlay = document.getElementById("modalOverlay");
function hideAdminModal() {
    const modalElement = document.getElementById("adminModal");
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
        const username = document.getElementById("username").value;
        const password = document.getElementById("password").value;
        try {
            const loginResponse = await fetch("/api/Admin/Login", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ username, password })
            });
            if (!loginResponse.ok) {
                const errorData = await loginResponse.json();
                alert(errorData.message || "Nom d'utilisateur ou mot de passe incorrect.");
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




