const canvas = document.getElementById("roomCanvas");
const ctx = canvas.getContext("2d");
let tables = [];
let tablesReserved = [];
let selectedTableId = null;

// Fonction pour afficher ou masquer le message de chargement
function setLoadingMessage(visible) {
    document.getElementById("loadingMessage").style.display = visible ? "block" : "none";
}

// Fonction pour dessiner une table sur le canvas
function drawTable(table) {
    ctx.save();

    // Déplacer le contexte au centre de la table
    ctx.translate(table.x + table.width / 2, table.y + table.height / 2);

    // Si la table est tournée, appliquer une rotation
    if (table.rotated) {
        ctx.rotate(Math.PI / 2);
    }


    // Définir les couleurs en fonction de l'état (réservée ou libre)
    ctx.fillStyle = tablesReserved.includes(table.id) ? "#e6b0aa" : "#6D9F71";

    // Dessiner un rectangle avec coins arrondis
    const radius = 10; // Rayon pour arrondir les coins
    const x = -table.width / 2;
    const y = -table.height / 2;
    const width = table.width;
    const height = table.height;

    ctx.beginPath();
    ctx.moveTo(x + radius, y); // Coin supérieur gauche
    ctx.lineTo(x + width - radius, y); // Ligne supérieure
    ctx.quadraticCurveTo(x + width, y, x + width, y + radius); // Coin supérieur droit
    ctx.lineTo(x + width, y + height - radius); // Ligne droite
    ctx.quadraticCurveTo(x + width, y + height, x + width - radius, y + height); // Coin inférieur droit
    ctx.lineTo(x + radius, y + height); // Ligne inférieure
    ctx.quadraticCurveTo(x, y + height, x, y + height - radius); // Coin inférieur gauche
    ctx.lineTo(x, y + radius); // Ligne gauche
    ctx.quadraticCurveTo(x, y, x + radius, y); // Coin supérieur gauche
    ctx.closePath();

    // Ajout d'une ombre pour un effet visuel
    ctx.shadowColor = "rgba(0, 0, 0, 0.3)";
    ctx.shadowBlur = 8;
    ctx.shadowOffsetX = 4;
    ctx.shadowOffsetY = 4;

    // Remplir le rectangle avec la couleur
    ctx.fill();

    // Réinitialiser l'ombre avant de dessiner le texte
    ctx.shadowColor = "transparent";

    // Dessiner le nom de la table
    ctx.fillStyle = "white"; // Blanc pour un meilleur contraste
    ctx.font = "bold 14px Arial";
    ctx.textAlign = "center";
    ctx.textBaseline = "middle";
    ctx.fillText(table.name, 0, -8); // Texte principal légèrement au-dessus du centre

    // Dessiner le nombre de sièges
    ctx.font = "12px Arial";
    ctx.fillText(`(${table.seats} P)`, 0, 10); // Texte secondaire légèrement en dessous du centre

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
                reserved: false // Initialisation à non réservé
            }));
            console.log("Tables chargées :", tables);
            drawTables(); // Dessine les tables après le chargement
        })
        .catch(error => console.error("Erreur lors du chargement des tables :", error))
        .finally(() => setLoadingMessage(false));
}

// Fonction pour vérifier s'il existe une réservation à une date et une heure
function checkReservation(date, time) {
    fetch(`/api/reservations?date=${date}&time=${time}`)
        .then(response => response.json())
        .then(data => {
            if (!data || !Array.isArray(data)) {
                console.error("Format de réponse incorrect :", data);
                return;
            }

            tables.forEach(table => {
                // Vérifie si cette table est réservée
                tablesReserved = data.map(reservation => reservation.tableId);
            });

            // Redessiner les tables après mise à jour des réservations
            drawTables();
        })
        .catch(error => {
            console.error("Erreur lors de la vérification des réservations :", error);
        });
}



// Initialisation de la page et gestion de la sélection de la date et de l'heure
document.addEventListener("DOMContentLoaded", function () {
    const selectDateInput = document.getElementById("selectDate");
    const startTimeSelect = document.getElementById("startTime");

    const today = new Date();
    const tomorrow = new Date(today);
    tomorrow.setDate(today.getDate() + 1);
    const minDate = tomorrow.toISOString().split("T")[0];
    selectDateInput.setAttribute("min", minDate);

    // Gestion du changement de date // bug table reste rouge au changement de date = fixé par appel 2 fonctions
    selectDateInput.addEventListener("change", function () {
        const selectedTime = startTimeSelect.value;
        const selectedDate = selectDateInput.value;
        if (!selectedDate) {
            alert("Veuillez sélectionner une date.");
            return;
        }

        loadTablesForDate(selectedDate);
        checkReservation(selectedDate, selectedTime);

    });

    // Gestion du changement d'heure
    startTimeSelect.addEventListener("change", function () {
        const selectedTime = startTimeSelect.value;
        const selectedDate = selectDateInput.value;

        if (!selectedDate || !selectedTime) {
            alert("Veuillez sélectionner une date et une heure.");
            return;
        }

        // Appel de la fonction pour vérifier les réservations
        checkReservation(selectedDate, selectedTime);
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
    if (clickedTable ) {
        console.log("Table cliquée :", clickedTable);


        if (tablesReserved.includes(clickedTable.id)) {
            console.log("Table déjà réservée.");
            alert("Cette table est déjà réservée.");
        } else {
            console.log("Table cliquée :", clickedTable);
            openReservationModal(clickedTable); // Ouvre la modale pour réserver la table
        }
    } else {
        console.log("Aucune table disponible cliquée.");
    }
});

// Fonction pour ouvrir la fenêtre modale
function openReservationModal(table) {
    selectedTableId = table.id;
    selectedTableName = table.name;
    const selectedDate = document.getElementById("selectDate").value;
    const startTime = document.getElementById("startTime").value;
    const TableNameModal = document.getElementById("TableNameModal").value;

    

    if (!selectedDate) {
        alert("Veuillez sélectionner une date avant de réserver une table.");
        return;
    }
    // Renseigner les informations dans la modale
    document.getElementById("tableId").textContent = table.id;
    document.getElementById("displayReservationDate").textContent = selectedDate;
    document.getElementById("displayReservationHoure").textContent = startTime;
    document.getElementById("TableNameModal").textContent = selectedTableName;


    
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

    // Vérification des champs vides
    if (!reservationDate || !startTime || !clientName || !clientEmail || !clientPhone) {
        alert("Veuillez remplir tous les champs.");
        return;
    }

    // Vérification de l'email (Format valide)
    const emailPattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    if (!emailPattern.test(clientEmail)) {
        alert("Veuillez entrer une adresse e-mail valide.");
        return;
    }

    // Vérification du numéro de téléphone (Format international ou local)
    const phonePattern = /^(\+?\d{1,4}[\s.-]?)?(\(?\d{2,4}\)?[\s.-]?)?[\d\s.-]{6,15}$/;
    if (!phonePattern.test(clientPhone)) {
        alert("Veuillez entrer un numéro de téléphone valide.");
        return;
    }

    // Création de l'objet de réservation
    const reservationData = {
        TableId: selectedTableId,
        TableName: selectedTableName,
        Date: reservationDate,
        StartTime: startTime,
        ClientName: clientName,
        ClientEmail: clientEmail,
        ClientPhone: clientPhone,
        SuccessUrl: window.location.origin + "/success",
        CancelUrl: window.location.origin + "/cancel"
    };

    // Envoi de la requête au serveur
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


//script pour le menu deroulant administrateur connexion
function navigateToPage() {
    const menu = document.getElementById("adminMenu");
    const selectedOption = menu.value;
    if (selectedOption) {
        window.location.href = selectedOption; // Redirige vers la page sélectionnée
    }
}


