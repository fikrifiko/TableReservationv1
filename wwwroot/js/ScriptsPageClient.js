const canvas = document.getElementById("roomCanvas");
const ctx = canvas.getContext("2d");
let tables = [];
<<<<<<< HEAD
let tablesReserved = [];
let selectedTableId = null;

// loading message
function setLoadingMessage(visible) {
    document.getElementById("loadingMessage").style.display = visible ? "block" : "none";
}

// draw table
function drawTable(table) {
    ctx.save();

    // center transform
    ctx.translate(table.x + table.width / 2, table.y + table.height / 2);

    // rotate if needed
=======
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
>>>>>>> old-origin/master
    if (table.rotated) {
        ctx.rotate(Math.PI / 2);
    }

<<<<<<< HEAD

    // color by reserved state
    ctx.fillStyle = tablesReserved.includes(table.id) ? "#e6b0aa" : "#6D9F71";

    // rounded rect
    const radius = 10;
    const x = -table.width / 2;
    const y = -table.height / 2;
    const width = table.width;
    const height = table.height;

    ctx.beginPath();
    ctx.moveTo(x + radius, y);
    ctx.lineTo(x + width - radius, y);
    ctx.quadraticCurveTo(x + width, y, x + width, y + radius);
    ctx.lineTo(x + width, y + height - radius);
    ctx.quadraticCurveTo(x + width, y + height, x + width - radius, y + height);
    ctx.lineTo(x + radius, y + height);
    ctx.quadraticCurveTo(x, y + height, x, y + height - radius);
    ctx.lineTo(x, y + radius);
    ctx.quadraticCurveTo(x, y, x + radius, y);
    ctx.closePath();

    // shadow
    ctx.shadowColor = "rgba(0, 0, 0, 0.3)";
    ctx.shadowBlur = 8;
    ctx.shadowOffsetX = 4;
    ctx.shadowOffsetY = 4;

    // fill
    ctx.fill();

    // reset shadow
    ctx.shadowColor = "transparent";

    // label
    ctx.fillStyle = "white";
    ctx.font = "bold 14px Arial";
    ctx.textAlign = "center";
    ctx.textBaseline = "middle";
    ctx.fillText(table.name, 0, -8);

    // seats
    ctx.font = "12px Arial";
    ctx.fillText(`(${table.seats} P)`, 0, 10);
=======
    // Dessiner la table (rectangle)
    ctx.fillStyle = table.reserved ? "red" : "green";
    ctx.fillRect(-table.width / 2, -table.height / 2, table.width, table.height);

    // Récupérer le nom de la table
    const TableName = table.name || `Table ${table.id}`;

    // Dessiner le texte centré sur la table
    ctx.fillStyle = "black";
    ctx.font = "14px Arial";
    ctx.textAlign = "center";
    ctx.textBaseline = "middle";
    ctx.fillText(TableName, 0, 0); // 0, 0 est le centre après translation
>>>>>>> old-origin/master

    ctx.restore();
}

<<<<<<< HEAD

// redraw all
function drawTables() {
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    tables.forEach(table => drawTable(table));
}

// load tables by date
=======
// Fonction pour redessiner toutes les tables
function drawTables() {
    ctx.clearRect(0, 0, canvas.width, canvas.height); // Efface le canvas
    tables.forEach(table => drawTable(table)); // Dessine chaque table
}
// Fonction pour charger les tables en fonction de la date sélectionnée
>>>>>>> old-origin/master
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
<<<<<<< HEAD
                reserved: false // Initialisation à non réservé
            }));
            drawTables();
=======
            }));
            console.log("Tables chargées :", tables);
            drawTables(); // Dessine les tables après le chargement


>>>>>>> old-origin/master
        })
        .catch(error => console.error("Erreur lors du chargement des tables :", error))
        .finally(() => setLoadingMessage(false));
}
<<<<<<< HEAD

// check reservations
function checkReservation(date, time) {
    fetch(`/api/reservations?date=${date}&time=${time}`)
        .then(response => response.json())
        .then(data => {
            if (!data || !Array.isArray(data)) {
                console.error("Format de réponse incorrect :", data);
                return;
            }

            tables.forEach(table => {
                tablesReserved = data.map(reservation => reservation.tableId);
            });

            // Redessiner les tables après mise à jour des réservations
            drawTables();
        })
        .catch(error => {
            console.error("Erreur lors de la vérification des réservations :", error);
        });
}



// init + date/time handlers
document.addEventListener("DOMContentLoaded", function () {
    const selectDateInput = document.getElementById("selectDate");
    const startTimeSelect = document.getElementById("startTime");

=======
// Initialisation de la page et gestion de la sélection de la date
document.addEventListener("DOMContentLoaded", function () {
    const selectDateInput = document.getElementById("selectDate");
>>>>>>> old-origin/master
    const today = new Date();
    const tomorrow = new Date(today);
    tomorrow.setDate(today.getDate() + 1);
    const minDate = tomorrow.toISOString().split("T")[0];
    selectDateInput.setAttribute("min", minDate);
<<<<<<< HEAD

    // date change (fix redraw)
    selectDateInput.addEventListener("change", function () {
        const selectedTime = startTimeSelect.value;
        const selectedDate = selectDateInput.value;
=======
    selectDateInput.addEventListener("change", function () {
        const selectedDate = this.value;
>>>>>>> old-origin/master
        if (!selectedDate) {
            alert("Veuillez sélectionner une date.");
            return;
        }
<<<<<<< HEAD

        loadTablesForDate(selectedDate);
        checkReservation(selectedDate, selectedTime);

    });

    // time change
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

    // load canvas
=======
        loadTablesForDate(selectedDate);
    });
    // Charger les dimensions du canvas
>>>>>>> old-origin/master
    setLoadingMessage(true);
    fetch("/api/canvas/get")
        .then(response => response.json())
        .then(data => {
            canvas.width = data.width || 800;
            canvas.height = data.height || 600;
<<<<<<< HEAD
            drawTables();
        })
        .catch(error => console.error("Erreur lors du chargement des dimensions du canvas :", error))
        .finally(() => setLoadingMessage(false));

    populateStartTimeDropdown();
});

// populate time dropdown
function populateStartTimeDropdown() {
    const startTimeDropdown = document.getElementById("startTime");
    for (let hour = 15; hour <= 21; hour++) {
        const option = document.createElement("option");
        option.value = `${hour.toString().padStart(2, "0")}:00`;
        option.textContent = `${hour}:00`;
        startTimeDropdown.appendChild(option);
    }
}

// canvas click select
canvas.addEventListener("click", event => {
    const rect = canvas.getBoundingClientRect();
    const scaleX = canvas.width / rect.width;
    const scaleY = canvas.height / rect.height;
=======
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
>>>>>>> old-origin/master
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
<<<<<<< HEAD
    if (clickedTable ) {
        console.log("Table cliquée :", clickedTable);


        if (tablesReserved.includes(clickedTable.id)) {
            console.log("Table déjà réservée.");
            alert("Cette table est déjà réservée.");
        } else {
            console.log("Table cliquée :", clickedTable);
            openReservationModal(clickedTable);
        }
    } else {
        console.log("Aucune table cliquée.");
    }
});

// open modal
=======
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
>>>>>>> old-origin/master
function openReservationModal(table) {
    selectedTableId = table.id;
    selectedTableName = table.name;
    const selectedDate = document.getElementById("selectDate").value;
<<<<<<< HEAD
    const startTime = document.getElementById("startTime").value;
    const TableNameModal = document.getElementById("TableNameModal").value;

    

=======
>>>>>>> old-origin/master
    if (!selectedDate) {
        alert("Veuillez sélectionner une date avant de réserver une table.");
        return;
    }
<<<<<<< HEAD
    // fill modal
    document.getElementById("tableId").textContent = table.id;
    document.getElementById("displayReservationDate").textContent = selectedDate;
    document.getElementById("displayReservationHoure").textContent = startTime;
    document.getElementById("TableNameModal").textContent = selectedTableName;


    
    // show modal
    document.getElementById("reservationModal").style.display = "flex";
}

// close modal
function closeReservationModal() {
    document.getElementById("reservationModal").style.display = "none";
}

// submit reservation
=======
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
>>>>>>> old-origin/master
function submitReservation() {
    const reservationDate = document.getElementById("selectDate").value;
    const startTime = document.getElementById("startTime").value;
    const clientName = document.getElementById("clientName").value;
    const clientEmail = document.getElementById("clientEmail").value;
    const clientPhone = document.getElementById("clientPhone").value;
<<<<<<< HEAD

    // validate required
=======
    const TableName = "";


>>>>>>> old-origin/master
    if (!reservationDate || !startTime || !clientName || !clientEmail || !clientPhone) {
        alert("Veuillez remplir tous les champs.");
        return;
    }
<<<<<<< HEAD

    // validate email
    const emailPattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    if (!emailPattern.test(clientEmail)) {
        alert("Veuillez entrer une adresse e-mail valide.");
        return;
    }

    // validate phone
    const phonePattern = /^(\+?\d{1,4}[\s.-]?)?(\(?\d{2,4}\)?[\s.-]?)?[\d\s.-]{6,15}$/;
    if (!phonePattern.test(clientPhone)) {
        alert("Veuillez entrer un numéro de téléphone valide.");
        return;
    }

    // payload
=======
>>>>>>> old-origin/master
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
<<<<<<< HEAD

    // send
=======
>>>>>>> old-origin/master
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
<<<<<<< HEAD
}


//script pour le menu deroulant administrateur connexion
function navigateToPage() {
    const menu = document.getElementById("adminMenu");
    const selectedOption = menu.value;
    if (selectedOption) {
        window.location.href = selectedOption; // Redirige vers la page sélectionnée
    }
}


=======
}
>>>>>>> old-origin/master
