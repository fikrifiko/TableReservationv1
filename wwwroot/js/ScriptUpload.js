document.addEventListener("DOMContentLoaded", () => {
    const form = document.querySelector("form.upload-form");
    if (!form) return;
    form.addEventListener("submit", async (e) => {
        const fr = document.getElementById("file-fr");
        const en = document.getElementById("file-en");
        const nl = document.getElementById("file-nl");
        if (!fr?.files?.length || !en?.files?.length || !nl?.files?.length) {
            e.preventDefault();
            alert("Veuillez sélectionner les 3 fichiers PDF (Français, Anglais, Néerlandais).");
            return;
        }
        // Laisser le navigateur soumettre; on gère les erreurs serveur via redirection/message
        // Option: on peut intercepter la réponse fetch si on passait en XHR
    });
});

