
    window.onload = function () {
        const logoutButton = document.getElementById("btnAdminLogout");

    if (logoutButton) {
        logoutButton.addEventListener("click", async function () {
            try {
                let response = await fetch('/api/Admin/Logout', { method: 'POST' });

                if (response.ok) {
                    console.log("✅ Déconnexion réussie !");

                    console.log("🔹 Token supprimé :", localStorage.getItem("token")); // Vérification

                    // 🔹 Supprimer le token
                    localStorage.removeItem("token");


                    // 🔹 Redirection après 500ms pour éviter un bug
                    setTimeout(() => {
                        window.location.replace("/Home/Index");
                    }, 500);
                } else {
                    console.error("❌ Erreur lors de la déconnexion.");
                }
            } catch (error) {
                console.error("🚨 Erreur de connexion avec le serveur :", error);
            }
        });
        } else {
        console.error("⚠️ Bouton de déconnexion non trouvé !");
        }
    };


