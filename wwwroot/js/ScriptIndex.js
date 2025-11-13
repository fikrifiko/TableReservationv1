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
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        tables.forEach(table => {
            ctx.save();
            ctx.translate(table.x + table.width / 2, table.y + table.height / 2);

            if (table.rotated) {
                ctx.rotate(Math.PI / 2);
            }

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

            ctx.shadowColor = "rgba(0, 0, 0, 0.3)";
            ctx.shadowBlur = 8;
            ctx.shadowOffsetX = 4;
            ctx.shadowOffsetY = 4;

            ctx.fillStyle = "#6D9F71";
            ctx.fill();

            ctx.shadowColor = "transparent";

            ctx.fillStyle = "white";
            ctx.font = "bold 14px Arial";
            ctx.textAlign = "center";
            ctx.textBaseline = "middle";
            ctx.fillText(table.name, 0, -8);

            ctx.font = "12px Arial";
            ctx.fillText(`(${table.seats} P)`, 0, 10);

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
