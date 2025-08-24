
    window.onload = function () {
        const logoutButton = document.getElementById("btnAdminLogout");

    if (logoutButton) {
        logoutButton.addEventListener("click", async function () {
            try {
                let response = await fetch('/api/Admin/Logout', { method: 'POST' });

                if (response.ok) {
                    // Redirection immédiate
                    window.location.replace("/Home/Index?message=deconnected");
                } else {
                    console.error("Erreur lors de la déconnexion.");
                }
            } catch (error) {
                console.error("Erreur de connexion avec le serveur :", error);
            }
        });
        } else {
        console.error("Bouton de déconnexion non trouvé");
        }
    };


