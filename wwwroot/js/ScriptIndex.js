document.addEventListener("DOMContentLoaded", async () => {
    const langCode = (document.documentElement.lang || "fr").toLowerCase();
    const isNl = langCode.startsWith("nl");
    const strings = {
        fr: {
            invalidSeats: "Veuillez entrer un nombre valide (2, 4, 6, 8 ou 10).",
            rotateSelect: "Veuillez sélectionner une table pour la faire pivoter.",
            deleteSelect: "Veuillez sélectionner une table à supprimer.",
            saveTablesSuccess: "Tables enregistrées avec succès !",
            saveTablesError: "Erreur lors de l'enregistrement.",
            canvasSizeUnavailable: "La taille du canvas n'est pas disponible.",
            genericErrorPrefix: "Erreur :",
            loadTablesError: "Erreur lors du chargement des tables :",
            loadCanvasError: "Erreur lors du chargement de la taille du canvas :",
            canvasSaveSuccess: "Dimensions du canvas enregistrées avec succès !",
            canvasSaveError: "Erreur lors de l'enregistrement des dimensions.",
            selectTableClick: "Veuillez sélectionner une table en cliquant dessus.",
            enterNewName: "Veuillez entrer un nouveau nom pour la table.",
            tableRenameSuccess: name => `Le nom de la table a été mis à jour en "${name}".`,
            tableRenameErrorPrefix: "Erreur lors de la sauvegarde dans la base de données :"
        },
        nl: {
            invalidSeats: "Voer een geldig aantal in (2, 4, 6, 8 of 10).",
            rotateSelect: "Selecteer een tafel om deze te draaien.",
            deleteSelect: "Selecteer een tafel om te verwijderen.",
            saveTablesSuccess: "Tafels succesvol opgeslagen!",
            saveTablesError: "Fout bij het opslaan.",
            canvasSizeUnavailable: "De canvasgrootte is niet beschikbaar.",
            genericErrorPrefix: "Fout:",
            loadTablesError: "Fout bij het laden van de tafels:",
            loadCanvasError: "Fout bij het laden van de canvasgrootte:",
            canvasSaveSuccess: "Canvasafmetingen succesvol opgeslagen!",
            canvasSaveError: "Fout bij het opslaan van de afmetingen.",
            selectTableClick: "Selecteer een tafel door erop te klikken.",
            enterNewName: "Voer een nieuwe naam voor de tafel in.",
            tableRenameSuccess: name => `De tafelnaam is bijgewerkt naar "${name}".`,
            tableRenameErrorPrefix: "Fout bij het opslaan in de database:"
        }
    };
    const t = isNl ? strings.nl : strings.fr;

    const canvas = document.getElementById("roomCanvas");
    const ctx = canvas.getContext("2d");
    const sizeSelector = document.getElementById("canvasSizeSelect");
    const modifyNameButton = document.getElementById("modifyTableNameButton");
    const saveNameButton = document.getElementById("saveTableNameButton");
    const addTableButton = document.getElementById("addTableButton");
    const deleteTableButton = document.getElementById("deleteTableButton");
    const saveTablesButton = document.getElementById("saveButton");
    const rotateButton = document.getElementById("rotateTableButton");
    const saveCanvasButton = document.getElementById("saveCanvasButton");

    rotateButton.addEventListener("click", rotateTable);

    let tables = [];
    let draggingTable = null;
    let selectedTable = null;

    function drawTables() {
        // Fond élégant avec dégradé subtil
        const gradient = ctx.createLinearGradient(0, 0, canvas.width, canvas.height);
        gradient.addColorStop(0, "#f5f5f0");
        gradient.addColorStop(1, "#ffffff");
        ctx.fillStyle = gradient;
        ctx.fillRect(0, 0, canvas.width, canvas.height);
        
        tables.forEach(table => {
            ctx.save();
            ctx.translate(table.x + table.width / 2, table.y + table.height / 2);

            if (table.rotated) {
                ctx.rotate(Math.PI / 2);
            }

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
            woodGradient.addColorStop(0, "#d4c5a5");
            woodGradient.addColorStop(0.3, "#c4b595");
            woodGradient.addColorStop(0.6, "#b4a585");
            woodGradient.addColorStop(1, "#a49575");
            ctx.fillStyle = woodGradient;
            ctx.fill();
            
            // Bordure du dessus
            ctx.strokeStyle = "#948565";
            ctx.lineWidth = 2;
            ctx.stroke();
            
            // Lignes de texture bois (subtiles)
            ctx.strokeStyle = "rgba(148, 133, 101, 0.3)";
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
            
            ctx.fillStyle = "#8B9A7F";
            ctx.strokeStyle = "#6B7A5F";
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
        });
    }

    function loadTables() {
        fetch("/api/tables")
            .then(response => response.json())
            .then(data => {
                tables = data.map(table => ({
                    ...table,
                    selected: false
                }));
                drawTables();
            })
            .catch(error => console.error(t.loadTablesError, error));
    }

    function resizeCanvas(scale) {
        const originalWidth = 800;
        canvas.width = originalWidth * scale;
        canvas.height = 600;
        drawTables();
    }

    sizeSelector.addEventListener("change", event => {
        const selectedSize = event.target.value;

        if (selectedSize === "1") {
            canvas.width = 800;
            canvas.height = 600;
        } else if (selectedSize === "1.50") {
            canvas.width = 1200;
            canvas.height = 600;
        } else if (selectedSize === "1.90") {
            canvas.width = 1520;
            canvas.height = 600;
        }

        console.log(`Canvas dimensions: ${canvas.width}x${canvas.height}`);
        drawTables();
    });

    addTableButton.addEventListener("click", () => {
        const seats = parseInt(document.getElementById("seatsInput").value);
        if (seats < 2 || seats > 10 || seats % 2 !== 0) {
            alert(t.invalidSeats);
            return;
        }
        const newTable = {
            id: tables.length + 1,
            name: `${isNl ? "Tafel" : "Table"} ${tables.length + 1}`,
            x: 100,
            y: 100,
            width: 50 * (seats / 2),
            height: 50,
            seats: seats,
            rotated: false,
            selected: false
        };
        tables.push(newTable);
        drawTables();
    });

    function rotateTable() {
        if (!selectedTable) {
            alert(t.rotateSelect);
            return;
        }
        selectedTable.rotated = !selectedTable.rotated;
        drawTables();
    }

    deleteTableButton.addEventListener("click", () => {
        if (!selectedTable) {
            alert(t.deleteSelect);
            return;
        }
        tables = tables.filter(table => table !== selectedTable);
        selectedTable = null;
        drawTables();
    });

    saveTablesButton.addEventListener("click", () => {
        const dataToSend = tables.map((table, index) => {
            const resolveNumber = (primary, secondary, defaultValue = 0) => {
                const value = Number.isFinite(primary) ? primary : Number.isFinite(secondary) ? secondary : defaultValue;
                return Math.round(value);
            };

            const resolveBoolean = (primary, secondary) => {
                if (typeof primary === "boolean") return primary;
                if (typeof secondary === "boolean") return secondary;
                return false;
            };

            const rawName = typeof table.name === "string" ? table.name
                : typeof table.Name === "string" ? table.Name
                : "";
            const normalizedName = rawName.trim().length > 0
                ? rawName.trim()
                : `${isNl ? "Tafel" : "Table"} ${index + 1}`;

            return {
                x: resolveNumber(table.x, table.X),
                y: resolveNumber(table.y, table.Y),
                width: resolveNumber(table.width, table.Width),
                height: resolveNumber(table.height, table.Height),
                seats: Number.isFinite(table.seats) ? table.seats : Number.isFinite(table.Seats) ? table.Seats : 2,
                rotated: resolveBoolean(table.rotated, table.Rotated),
                reserved: resolveBoolean(table.reserved, table.Reserved),
                name: normalizedName
            };
        });

        fetch("/api/tables", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(dataToSend)
        })
            .then(response => {
                if (response.ok) alert(t.saveTablesSuccess);
                else throw new Error(t.saveTablesError);
            })
            .catch(error => alert(`${t.genericErrorPrefix} ${error.message}`));
    });

    async function loadCanvasSize() {
        try {
            const response = await fetch("/api/canvas/get");
            const data = await response.json();
            if (data.width && data.height) {
                canvas.width = data.width;
                canvas.height = data.height;
                return { width: data.width, height: data.height };
            }
            throw new Error(t.canvasSizeUnavailable);
        } catch (error) {
            console.error(t.loadCanvasError, error);
            return { width: 800, height: 600 };
        }
    }

    saveCanvasButton.addEventListener("click", () => {
        const dataToSend = {
            width: canvas.width,
            height: canvas.height
        };

        fetch("/api/canvas/save", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(dataToSend)
        })
            .then(response => {
                if (response.ok) {
                    alert(t.canvasSaveSuccess);
                } else {
                    throw new Error(t.canvasSaveError);
                }
            })
            .catch(error => {
                console.error(t.genericErrorPrefix, error.message);
                alert(`${t.genericErrorPrefix} ${error.message}`);
            });
    });

    canvas.addEventListener("mousedown", e => {
        const rect = canvas.getBoundingClientRect();
        const mouseX = e.clientX - rect.left;
        const mouseY = e.clientY - rect.top;

        const clickedTable = tables.find(
            table =>
                mouseX >= table.x &&
                mouseX <= table.x + table.width &&
                mouseY >= table.y &&
                mouseY <= table.y + table.height
        );

        if (clickedTable) {
            draggingTable = clickedTable;
            selectedTable = clickedTable;
            tables.forEach(table => (table.selected = false));
            clickedTable.selected = true;
        } else {
            selectedTable = null;
            tables.forEach(table => (table.selected = false));
        }
        drawTables();
    });

    canvas.addEventListener("mousemove", e => {
        if (!draggingTable) return;
        const rect = canvas.getBoundingClientRect();
        draggingTable.x = e.clientX - rect.left - draggingTable.width / 2;
        draggingTable.y = e.clientY - rect.top - draggingTable.height / 2;
        drawTables();
    });

    canvas.addEventListener("mouseup", () => {
        draggingTable = null;
    });

    canvas.addEventListener("mouseleave", () => {
        draggingTable = null;
    });

    modifyNameButton.addEventListener("click", () => {
        if (!selectedTable) {
            alert(t.selectTableClick);
            return;
        }

        document.getElementById("tableNameInput").value = selectedTable.name || "";

        const tableNameModal = new bootstrap.Modal(document.getElementById("tableNameModal"));
        tableNameModal.show();
    });

    saveNameButton.addEventListener("click", () => {
        const newName = document.getElementById("tableNameInput").value.trim();

        if (!newName) {
            alert(t.enterNewName);
            return;
        }

        if (selectedTable) {
            selectedTable.name = newName;

            fetch(`/api/tables/${selectedTable.id}`, {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({ name: newName })
            })
                .then(response => {
                    if (response.ok) {
                        drawTables();
                        alert(t.tableRenameSuccess(newName));
                        const tableNameModal = bootstrap.Modal.getInstance(document.getElementById("tableNameModal"));
                        tableNameModal.hide();
                    } else {
                        return response.json().then(error => {
                            throw new Error(error.message || t.saveTablesError);
                        });
                    }
                })
                .catch(error => {
                    console.error(t.genericErrorPrefix, error);
                    alert(`${t.tableRenameErrorPrefix} ${error.message}`);
                });
        }
    });

    const defaultCanvasSize = await loadCanvasSize();
    canvas.width = defaultCanvasSize.width;
    canvas.height = defaultCanvasSize.height;
    loadTables();
});
