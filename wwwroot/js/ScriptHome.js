// Gestion du bouton "Se connecter"
const btnAdminLogin = document.getElementById("btnAdminLogin");

btnAdminLogin.addEventListener("click", function (event) {
    event.preventDefault();
    console.log("✅ Bouton 'Se connecter' cliqué !");

    let modalElement = document.getElementById("adminModal");
    let modal = new bootstrap.Modal(modalElement); // Utilise Bootstrap pour gérer le modal
    modal.show();
});

// Ouvrir la modale admin avec Bootstrap
function openAdminModal() {
    let modalElement = document.getElementById("adminModal");
    let modal = new bootstrap.Modal(modalElement);
    modal.show();
}

// Fermer la modale admin avec Bootstrap
function closeAdminModal() {
    let modalElement = document.getElementById("adminModal");
    let modal = bootstrap.Modal.getInstance(modalElement);
    if (modal) {
        modal.hide();
        console.log("✅ Modal fermé !");
    } else {
        console.error("⚠️ Erreur : Impossible de fermer le modal.");
    }
}

// Soumettre le formulaire de connexion
async function submitAdminLogin(event) {
    event.preventDefault();

    const username = document.getElementById("username").value;
    const password = document.getElementById("password").value;

    try {
        // 🔹 POST pour récupérer le token
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
            console.log("✅ Connexion réussie, token reçu :", loginData.token);

            // 🔹 Stocker le token dans localStorage
            localStorage.setItem("token", loginData.token);

            // ✅ GET vers AdminView/Dashboard avec le Bearer JWT
            const dashboardResponse = await fetch("/AdminView/Dashboard", {
                method: "GET",
                headers: {
                    "Authorization": `Bearer ${localStorage.getItem("token")}`,
                    "Content-Type": "application/json"
                }
            });

            if (!dashboardResponse.ok) {
                throw new Error("🚨 Accès refusé : " + dashboardResponse.status);
            }

            // ✅ Rediriger uniquement si l'accès au Dashboard est autorisé
            window.location.href = "/AdminView/Dashboard"; // ✅ Corrigez bien ici !
        }
    } catch (error) {
        console.error("🚨 Erreur lors de la connexion :", error);
        alert("Une erreur est survenue. Veuillez réessayer.");
    }
}




