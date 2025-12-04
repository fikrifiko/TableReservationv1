const canvas = document.getElementById("roomCanvas");
const ctx = canvas.getContext("2d");
let tables = [];
let tablesReserved = [];
let selectedTableId = null;
let selectedTableName = null;
let loggedInClient = null;

// loading message
function setLoadingMessage(visible) {
    document.getElementById("loadingMessage").style.display = visible ? "block" : "none";
}

// draw table - style réaliste
function drawTable(table) {
    ctx.save();

    // center transform
    ctx.translate(table.x + table.width / 2, table.y + table.height / 2);

    // rotate if needed
    if (table.rotated) {
        ctx.rotate(Math.PI / 2);
    }

    const isReserved = tablesReserved.includes(table.id);
    const x = -table.width / 2;
    const y = -table.height / 2;
    const width = table.width;
    const height = table.height;
    const radius = Math.min(width, height) * 0.15; // Radius proportionnel

    // === OMBRE PORTÉE DE LA TABLE (sous la table) ===
    ctx.save();
    ctx.shadowColor = "rgba(0, 0, 0, 0.25)";
    ctx.shadowBlur = 15;
    ctx.shadowOffsetX = 0;
    ctx.shadowOffsetY = 8;
    
    // Ombre ovale sous la table
    ctx.beginPath();
    ctx.ellipse(0, height / 2 + 5, width * 0.6, height * 0.3, 0, 0, Math.PI * 2);
    ctx.fillStyle = "rgba(0, 0, 0, 0.15)";
    ctx.fill();
    ctx.restore();

    // === PIEDS DE TABLE (4 pieds) ===
    const legWidth = Math.max(4, width * 0.08);
    const legHeight = height * 0.3;
    const legOffset = Math.min(width, height) * 0.25;
    
    ctx.fillStyle = "#4a4a4a"; // Couleur métal foncé
    ctx.strokeStyle = "#2a2a2a";
    ctx.lineWidth = 1;
    
    // Pieds aux 4 coins
    const legs = [
        { x: -width / 2 + legOffset, y: height / 2 },
        { x: width / 2 - legOffset, y: height / 2 },
        { x: -width / 2 + legOffset, y: height / 2 - legHeight },
        { x: width / 2 - legOffset, y: height / 2 - legHeight }
    ];
    
    legs.forEach(leg => {
        // Ombre du pied
        ctx.save();
        ctx.shadowColor = "rgba(0, 0, 0, 0.3)";
        ctx.shadowBlur = 5;
        ctx.shadowOffsetX = 2;
        ctx.shadowOffsetY = 2;
        ctx.fillRect(leg.x - legWidth / 2, leg.y - legHeight, legWidth, legHeight);
        ctx.restore();
        
        // Reflet sur le pied
        ctx.fillStyle = "#6a6a6a";
        ctx.fillRect(leg.x - legWidth / 2, leg.y - legHeight, legWidth / 2, legHeight);
        ctx.fillStyle = "#4a4a4a";
    });

    // === PLANCHER DE TABLE (dessus) ===
    ctx.save();
    
    // Ombre portée du dessus
    ctx.shadowColor = "rgba(0, 0, 0, 0.2)";
    ctx.shadowBlur = 10;
    ctx.shadowOffsetX = 0;
    ctx.shadowOffsetY = 3;
    
    // Dessus de table avec coins arrondis
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

    // Texture bois avec dégradé réaliste
    const woodGradient = ctx.createLinearGradient(x, y, x + width, y + height);
    if (isReserved) {
        // Table réservée - teinte rougeâtre
        woodGradient.addColorStop(0, "#d4a5a5");
        woodGradient.addColorStop(0.3, "#c89595");
        woodGradient.addColorStop(0.6, "#b88888");
        woodGradient.addColorStop(1, "#a87a7a");
    } else {
        // Table disponible - teinte bois chaleureux
        woodGradient.addColorStop(0, "#d4c5a5");
        woodGradient.addColorStop(0.3, "#c4b595");
        woodGradient.addColorStop(0.6, "#b4a585");
        woodGradient.addColorStop(1, "#a49575");
    }
    ctx.fillStyle = woodGradient;
    ctx.fill();
    
    // Bordure du dessus
    ctx.strokeStyle = isReserved ? "#a87a7a" : "#948565";
    ctx.lineWidth = 2;
    ctx.stroke();
    
    // Lignes de texture bois (subtiles)
    ctx.strokeStyle = isReserved ? "rgba(168, 122, 122, 0.3)" : "rgba(148, 133, 101, 0.3)";
    ctx.lineWidth = 1;
    for (let i = 1; i < 4; i++) {
        ctx.beginPath();
        ctx.moveTo(x + (width / 4) * i, y + 2);
        ctx.lineTo(x + (width / 4) * i, y + height - 2);
        ctx.stroke();
    }
    
    ctx.restore();

    // === CHAISES AUTOUR DE LA TABLE ===
    const numChairs = table.seats;
    const chairRadius = Math.min(width, height) * 0.12;
    const chairDistance = Math.max(width, height) * 0.6;
    
    ctx.fillStyle = isReserved ? "#b88888" : "#8B9A7F";
    ctx.strokeStyle = isReserved ? "#a87a7a" : "#6B7A5F";
    ctx.lineWidth = 1.5;
    
    for (let i = 0; i < numChairs; i++) {
        const angle = (Math.PI * 2 * i) / numChairs;
        const chairX = Math.cos(angle) * chairDistance;
        const chairY = Math.sin(angle) * chairDistance;
        
        ctx.save();
        ctx.translate(chairX, chairY);
        
        // Ombre de la chaise
        ctx.shadowColor = "rgba(0, 0, 0, 0.2)";
        ctx.shadowBlur = 6;
        ctx.shadowOffsetX = 2;
        ctx.shadowOffsetY = 2;
        
        // Siège de la chaise (cercle)
        ctx.beginPath();
        ctx.arc(0, 0, chairRadius, 0, Math.PI * 2);
        ctx.fill();
        
        // Bordure du siège
        ctx.stroke();
        
        ctx.restore();
    }

    // === LABEL DE LA TABLE ===
    ctx.fillStyle = "#ffffff";
    ctx.font = "600 16px 'Cormorant Garamond', serif";
    ctx.textAlign = "center";
    ctx.textBaseline = "middle";
    
    // Ombre du texte
    ctx.shadowColor = "rgba(0, 0, 0, 0.5)";
    ctx.shadowBlur = 4;
    ctx.shadowOffsetX = 1;
    ctx.shadowOffsetY = 1;
    ctx.fillText(table.name, 0, -8);
    
    // Nombre de personnes
    ctx.font = "400 12px 'Cormorant Garamond', serif";
    ctx.fillText(`${table.seats} pers.`, 0, 10);
    
    // Reset shadow
    ctx.shadowColor = "transparent";

    ctx.restore();
}


// redraw all avec fond élégant
function drawTables() {
    // Fond élégant avec dégradé subtil
    const gradient = ctx.createLinearGradient(0, 0, canvas.width, canvas.height);
    gradient.addColorStop(0, "#fafafa");
    gradient.addColorStop(1, "#ffffff");
    ctx.fillStyle = gradient;
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    
    // Lignes de grille subtiles pour un effet élégant
    ctx.strokeStyle = "rgba(0, 0, 0, 0.03)";
    ctx.lineWidth = 1;
    const gridSize = 50;
    for (let x = 0; x <= canvas.width; x += gridSize) {
        ctx.beginPath();
        ctx.moveTo(x, 0);
        ctx.lineTo(x, canvas.height);
        ctx.stroke();
    }
    for (let y = 0; y <= canvas.height; y += gridSize) {
        ctx.beginPath();
        ctx.moveTo(0, y);
        ctx.lineTo(canvas.width, y);
        ctx.stroke();
    }
    
    // Dessiner les tables
    tables.forEach(table => drawTable(table));
}

// load tables by date
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
            drawTables();
        })
        .catch(error => console.error("Erreur lors du chargement des tables :", error))
        .finally(() => setLoadingMessage(false));
}

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
document.addEventListener("DOMContentLoaded", async function () {
    const selectDateInput = document.getElementById("selectDate");
    const startTimeSelect = document.getElementById("startTime");

    const today = new Date();
    const tomorrow = new Date(today);
    tomorrow.setDate(today.getDate() + 1);
    const minDate = tomorrow.toISOString().split("T")[0];
    selectDateInput.setAttribute("min", minDate);

    // date change (fix redraw)
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
    setLoadingMessage(true);
    fetch("/api/canvas/get")
        .then(response => response.json())
        .then(data => {
            canvas.width = data.width || 800;
            canvas.height = data.height || 600;
            drawTables();
        })
        .catch(error => console.error("Erreur lors du chargement des dimensions du canvas :", error))
        .finally(() => setLoadingMessage(false));

    populateStartTimeDropdown();

    // bind modal controls (no inline handlers)
    const closeBtn = document.getElementById("reservationCloseBtn");
    if (closeBtn) closeBtn.addEventListener("click", closeReservationModal);
    const cancelBtn = document.getElementById("cancelReserveBtn");
    if (cancelBtn) cancelBtn.addEventListener("click", closeReservationModal);
    const reserveBtn = document.getElementById("reserveBtn");
    if (reserveBtn) reserveBtn.addEventListener("click", submitReservation);
    const overlay = document.getElementById("reservationOverlay");
    if (overlay) overlay.addEventListener("click", closeReservationModal);

    await loadLoggedInClient();
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
            openReservationModal(clickedTable);
        }
    } else {
        console.log("Aucune table cliquée.");
    }
});

// open modal
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
    // fill modal
    document.getElementById("tableId").textContent = table.id;
    document.getElementById("displayReservationDate").textContent = selectedDate;
    document.getElementById("displayReservationHoure").textContent = startTime;
    document.getElementById("TableNameModal").textContent = selectedTableName;

    if (loggedInClient) {
        prefillClientFields();
    } else {
        loadLoggedInClient();
    }


    
    // show modal
    document.getElementById("reservationModal").style.display = "flex";
    const overlay = document.getElementById("reservationOverlay");
    if (overlay) overlay.style.display = "block";
}

// close modal
function closeReservationModal() {
    document.getElementById("reservationModal").style.display = "none";
    const overlay = document.getElementById("reservationOverlay");
    if (overlay) overlay.style.display = "none";
}

async function loadLoggedInClient() {
    try {
        const response = await fetch("/api/client/session", { headers: { "Accept": "application/json" } });
        if (!response.ok) {
            throw new Error("unauthorized");
        }
        const data = await response.json();
        loggedInClient = data;
        prefillClientFields();
    } catch (error) {
        loggedInClient = null;
    }
}

function prefillClientFields() {
    if (!loggedInClient) {
        return;
    }

    const nameInput = document.getElementById("clientName");
    const emailInput = document.getElementById("clientEmail");
    const phoneInput = document.getElementById("clientPhone");

    if (nameInput) {
        nameInput.value = loggedInClient.name || "";
    }
    if (emailInput) {
        emailInput.value = loggedInClient.email || "";
    }
    if (phoneInput) {
        phoneInput.value = loggedInClient.phone || "";
    }
}

// submit reservation
function submitReservation() {
    const reservationDate = document.getElementById("selectDate").value;
    const startTime = document.getElementById("startTime").value;
    const clientName = document.getElementById("clientName").value;
    const clientEmail = document.getElementById("clientEmail").value;
    const clientPhone = document.getElementById("clientPhone").value;

    // validate required
    if (!reservationDate || !startTime || !clientName || !clientEmail || !clientPhone) {
        alert("Veuillez remplir tous les champs.");
        return;
    }

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

    // send
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


