const canvas = document.getElementById("roomCanvas");
const ctx = canvas.getContext("2d");
let tables = [];
let selectedTableId = null;


const images = {
    2: new Image(),
    4: new Image(),
    6: new Image(),
    8: new Image(),
};

// Chargez les images
images[2].src = "./Images/table_2p.png"; // Chemin relatif depuis votre HTML ou JS
images[4].src = "./Images/table_4p.png";
images[6].src = "./Images/table_6p.png";
images[8].src = "./Images/table_8p.png";



// Facultatif : Ajoutez un log pour vérifier que les images sont bien chargées
images[2].onload = () => console.log("Image 2 places chargée.");
images[4].onload = () => console.log("Image 4 places chargée.");
images[6].onload = () => console.log("Image 6 places chargée.");
images[8].onload = () => console.log("Image 8 places chargée.");

// Fonction pour afficher ou masquer le message de chargement
function setLoadingMessage(visible) {
    document.getElementById("loadingMessage").style.display = visible ? "block" : "none";
}

// Fonction pour dessiner une table sur le canvas
function drawTable(table) {
    ctx.save();
    ctx.translate(table.x + table.width / 2, table.y + table.height / 2);

    if (table.rotated) {
        ctx.rotate(Math.PI / 2);
    }

    // Couleur de la table : verte si disponible, rouge si réservée
    ctx.fillStyle = table.reserved ? "red" : "green";
    ctx.fillRect(-table.width / 2, -table.height / 2, table.width, table.height);
     
    // Texte centré sur la table
    ctx.fillStyle = "black";
    ctx.font = "14px Arial";
    ctx.textAlign = "center";
    ctx.textBaseline = "middle";
    ctx.fillText(`Table ${table.id}`, 0, 0);

    ctx.restore();
}

// Fonction pour redessiner toutes les tables
function drawTables() {
    ctx.clearRect(0, 0, canvas.width, canvas.height); // Efface le canvas
    tables.forEach(table => drawTable(table)); // Dessine chaque table
}

// Fonction pour charger les tables en fonction de la date sélectionnée
function loadTablesForDate(date) {
    setLoadingMessage(true);

    fetch(`/api/tables?date=${date}`)
        .then(response => response.json())
        .then(data => {
            tables = data.map(table => ({
                ...table,
                x: table.x || 0,
                y: table.y || 0,
                width: table.width || 50,
                height: table.height || 50,
            }));
            console.log("Tables chargées :", tables);
            drawTables(); // Dessine les tables après le chargement
        })
        .catch(error => console.error("Erreur lors du chargement des tables :", error))
        .finally(() => setLoadingMessage(false));
}

// Initialisation de la page et gestion de la sélection de la date
document.addEventListener("DOMContentLoaded", function () {
    const selectDateInput = document.getElementById("selectDate");
    const today = new Date();
    const tomorrow = new Date(today);
    tomorrow.setDate(today.getDate() + 1);

    const minDate = tomorrow.toISOString().split("T")[0];
    selectDateInput.setAttribute("min", minDate);

    selectDateInput.addEventListener("change", function () {
        const selectedDate = this.value;
        if (!selectedDate) {
            alert("Veuillez sélectionner une date.");
            return;
        }
        loadTablesForDate(selectedDate);
    });

    // Charger les dimensions du canvas
    setLoadingMessage(true);
    fetch("/api/canvas/get")
        .then(response => response.json())
        .then(data => {
            canvas.width = data.width || 800;
            canvas.height = data.height || 600;
            drawTables(); // Dessine les tables après avoir mis à jour les dimensions
        })
        .catch(error => console.error("Erreur lors du chargement des dimensions du canvas :", error))
        .finally(() => setLoadingMessage(false));

    populateStartTimeDropdown();
});

// Gestion des clics sur le canvas pour sélectionner une table
canvas.addEventListener("click", event => {
    const rect = canvas.getBoundingClientRect();
    const scaleX = canvas.width / rect.width; // Echelle horizontale
    const scaleY = canvas.height / rect.height; // Echelle verticale

    const mouseX = (event.clientX - rect.left) * scaleX;
    const mouseY = (event.clientY - rect.top) * scaleY;

    console.log(`Clique détecté : (${mouseX}, ${mouseY})`);
    console.log("Tables disponibles :", tables);

    const clickedTable = tables.find(
        table =>
            mouseX >= table.x &&
            mouseX <= table.x + table.width &&
            mouseY >= table.y &&
            mouseY <= table.y + table.height
    );

    if (clickedTable && !clickedTable.reserved) {
        console.log("Table cliquée :", clickedTable);
        openReservationModal(clickedTable);
    } else {
        console.log("Aucune table disponible cliquée.");
    }
});

// Fonction pour remplir la liste déroulante des heures
function populateStartTimeDropdown() {
    const startTimeDropdown = document.getElementById("startTime");
    for (let hour = 15; hour <= 21; hour++) {
        const option = document.createElement("option");
        option.value = `${hour.toString().padStart(2, "0")}:00`;
        option.textContent = `${hour}:00`;
        startTimeDropdown.appendChild(option);
    }
}

// Fonction pour ouvrir la fenêtre modale
function openReservationModal(table) {
    selectedTableId = table.id;

    const selectedDate = document.getElementById("selectDate").value;
    if (!selectedDate) {
        alert("Veuillez sélectionner une date avant de réserver une table.");
        return;
    }

    // Renseigner les informations dans la modale
    document.getElementById("tableId").textContent = table.id;
    document.getElementById("displayReservationDate").textContent = selectedDate;

    // Afficher la modale
    document.getElementById("reservationModal").style.display = "flex";
}

// Fonction pour fermer la fenêtre modale
function closeReservationModal() {
    document.getElementById("reservationModal").style.display = "none";
}

// Fonction pour soumettre la réservation
function submitReservation() {
    const reservationDate = document.getElementById("selectDate").value;
    const startTime = document.getElementById("startTime").value;
    const clientName = document.getElementById("clientName").value;
    const clientEmail = document.getElementById("clientEmail").value;
    const clientPhone = document.getElementById("clientPhone").value;

    if (!reservationDate || !startTime || !clientName || !clientEmail || !clientPhone) {
        alert("Veuillez remplir tous les champs.");
        return;
    }

    const reservationData = {
        TableId: selectedTableId,
        Date: reservationDate,
        StartTime: startTime,
        ClientName: clientName,
        ClientEmail: clientEmail,
        ClientPhone: clientPhone,
        SuccessUrl: window.location.origin + "/success",
        CancelUrl: window.location.origin + "/cancel"
    };

    fetch("/api/payment/create-session", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(reservationData)
    })
        .then(response => {
            if (!response.ok) {
                throw new Error("Erreur lors de la création de la session de paiement.");
            }
            return response.json();
        })
        .then(data => {
            window.location.href = data.sessionUrl;
        })
        .catch(error => {
            console.error("Erreur :", error);
            alert("Erreur lors de la réservation : " + error.message);
        });
}
