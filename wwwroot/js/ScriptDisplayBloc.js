document.addEventListener("DOMContentLoaded", async () => {

    // pour donner accés que aux admins avec un token
    const token = localStorage.getItem("token");

    if (!token) {
        console.warn("🔒 Accès refusé : Aucun token trouvé, redirection vers la connexion !");
        window.location.href = "/Home/Index"; // Redirection si pas de token
    } else {
        document.body.style.display = "flex"; // 🔹 Affiche la page uniquement si token valide
    }
});