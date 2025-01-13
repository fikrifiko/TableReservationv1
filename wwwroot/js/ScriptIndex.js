
    // Variables pour le canvas
    const canvas = document.getElementById('roomCanvas');
    const ctx = canvas.getContext('2d');
    let tables = []; // Liste des tables
    let draggingTable = null; // Table en cours de déplacement
    let selectedTable = null; // Table actuellement sélectionnée

    // Fonction pour redimensionner le canvas
    function resizeCanvas(scale) {
        const originalWidth = 800; // Largeur par défaut
        canvas.width = originalWidth * scale; // Modifier la largeur
        canvas.height = 600; // Garder la hauteur constante
        drawTables(); // Redessiner les tables après redimensionnement
    }

    // Ajouter un événement pour gérer la taille du canvas
document.getElementById('canvasSizeSelect').addEventListener('change', (event) => {
    const selectedSize = event.target.value;

    // Appliquer les dimensions spécifiques pour chaque taille
    if (selectedSize === "1") {
        canvas.width = 800; // Taille par défaut
        canvas.height = 600;
    } else if (selectedSize === "1.50") {
        canvas.width = 1200; // Taille Moyenne
        canvas.height = 600;
    } else if (selectedSize === "1.90") {
        canvas.width = 1520; // Taille Large
        canvas.height = 600;
    }

    console.log(`Canvas dimensions: ${canvas.width}x${canvas.height}`); // Vérifiez les dimensions dans la console
    drawTables(); // Redessiner après redimensionnement
});


    // Fonction pour dessiner les tables
    function drawTables() {
        ctx.clearRect(0, 0, canvas.width, canvas.height); // Effacer le canvas
        tables.forEach(table => {
            ctx.save(); // Sauvegarder le contexte pour la rotation
            ctx.translate(table.x + table.width / 2, table.y + table.height / 2); // Positionner au centre

            // Rotation de la table
            if (table.rotated) {
                ctx.rotate(Math.PI / 2); // Pivoter de 90 degrés
            }

            // Dessiner la table
            ctx.fillStyle = table.selected ? 'gray' : 'blue'; // Couleur grise si sélectionnée
            ctx.fillRect(-table.width / 2, -table.height / 2, table.width, table.height); // Dessiner la table
            ctx.restore(); // Restaurer le contexte

            // Texte sans rotation
            ctx.fillStyle = 'white'; // Couleur du texte
            ctx.textAlign = 'center'; // Centrer le texte horizontalement
            ctx.textBaseline = 'middle'; // Centrer le texte verticalement
            ctx.fillText(`Table ${table.id}`, table.x + table.width / 2, table.y + table.height / 2 - 10);
            ctx.fillText(`(${table.seats} P)`, table.x + table.width / 2, table.y + table.height / 2 + 10);
        });
    }



    /** Ajouter une nouvelle table */
    function addTable() {
        const seats = parseInt(document.getElementById('seatsInput').value);
        if (seats < 2 || seats > 10 || seats % 2 !== 0) {
            alert('Veuillez entrer un nombre valide (2, 4, 6, 8 ou 10).');
            return;
        }
        const size = { width: 50 * (seats / 2), height: 50 };
        const newTable = {
            id: tables.length + 1,
            x: 100,
            y: 100,
            width: size.width,
            height: size.height,
            seats: seats,
            rotated: false,
            selected: false
        };
        tables.push(newTable);
        drawTables();
    }

    /** Faire pivoter la table sélectionnée */
    function rotateTable() {
        if (!selectedTable) {
            alert('Veuillez sélectionner une table pour la faire pivoter.');
            return;
        }
        selectedTable.rotated = !selectedTable.rotated;
        drawTables();
    }

    /** Supprimer la table sélectionnée */
    function deleteTable() {
        if (!selectedTable) {
            alert("Veuillez sélectionner une table à supprimer.");
            return;
        }

        // Envoi de la requête DELETE à l'API
        fetch(`/api/tables/${selectedTable.id}`, { // ID de la table sélectionnée
            method: 'DELETE',
        })
            .then(response => {
                if (response.ok) {
                    alert("Table supprimée avec succès !");
                    // Retirer la table du tableau local
                    tables = tables.filter(table => table.id !== selectedTable.id);
                    selectedTable = null; // Désélectionner la table
                    drawTables(); // Redessiner le canvas
                } else {
                    response.text().then(text => {
                        throw new Error(`Erreur lors de la suppression : ${text}`);
                    });
                }
            })
            .catch(error => {
                console.error("Erreur :", error);
                alert("Erreur lors de la suppression de la table.");
            });
    }


    /** Sauvegarder les tables dans la base de données */
    function saveTables() {
        const dataToSend = tables.map(({ x, y, width, height, seats, rotated }) => ({
            x: Math.round(x),
            y: Math.round(y),
            width: Math.round(width),
            height: Math.round(height),
            seats: seats,
            rotated: rotated
        }));
        fetch('/api/tables', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dataToSend)
        })
            .then((response) => {
                if (response.ok) alert('Tables enregistrées avec succès !');
                else throw new Error('Erreur lors de l\'enregistrement');
            })
            .catch((error) => alert(`Erreur : ${error.message}`));
    }

    /** Charger les tables depuis la base de données */
    function loadTables() {
        fetch('/api/tables', {
            method: 'GET',
            headers: { 'Content-Type': 'application/json' }
        })
            .then((response) => response.json())
            .then((data) => {
                tables = data;
                drawTables();
            })
            .catch((error) => alert(`Erreur lors du chargement : ${error.message}`));
    }



    /** sauver la taille du canvas  */
    function saveSizeCanvas() {
        // Récupérer le canvas
        const canvas = document.getElementById('roomCanvas');

        // Préparer les dimensions du canvas
        const dataToSend = {
            width: canvas.width,
            height: canvas.height
        };

        // Envoyer les dimensions via fetch à l'API
        fetch('/api/canvas/save', { // Remplacez '/api/canvas' par l'endpoint réel de votre API
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dataToSend)
        })
            .then(response => {
                if (response.ok) {
                    alert('Dimensions du canvas enregistrées avec succès !');
                } else {
                    throw new Error('Erreur lors de l\'enregistrement des dimensions');
                }
            })
            .catch(error => {
                console.error('Erreur :', error.message);
                alert(`Erreur : ${error.message}`);
            });
    
    }


    // Event Listeners
    document.getElementById('addTableButton').addEventListener('click', addTable);
    document.getElementById('rotateTableButton').addEventListener('click', rotateTable);
    document.getElementById('deleteTableButton').addEventListener('click', deleteTable);
    document.getElementById('saveButton').addEventListener('click', saveTables);
    document.getElementById('loadButton').addEventListener('click', loadTables);
    document.getElementById('saveCanvasButton').addEventListener('click', saveSizeCanvas);


    // Sélection et déplacement des tables
    canvas.addEventListener('mousedown', (e) => {
        const rect = canvas.getBoundingClientRect();
        const mouseX = e.clientX - rect.left;
        const mouseY = e.clientY - rect.top;
        const clickedTable = tables.find(
            (table) =>
                mouseX >= table.x &&
                mouseX <= table.x + table.width &&
                mouseY >= table.y &&
                mouseY <= table.y + table.height
        );
        if (clickedTable) {
            draggingTable = clickedTable;
            selectedTable = clickedTable;
            tables.forEach((table) => (table.selected = false));
            clickedTable.selected = true;
        } else {
            selectedTable = null;
            tables.forEach((table) => (table.selected = false));
        }
        drawTables();
    });

    canvas.addEventListener('mousemove', (e) => {
        if (!draggingTable) return;
        const rect = canvas.getBoundingClientRect();
        draggingTable.x = e.clientX - rect.left - draggingTable.width / 2;
        draggingTable.y = e.clientY - rect.top - draggingTable.height / 2;
        drawTables();
    });

    canvas.addEventListener('mouseup', () => {
        draggingTable = null;
    });

    canvas.addEventListener('mouseleave', () => {
        draggingTable = null;
    });
