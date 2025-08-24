document.addEventListener("DOMContentLoaded", async () => {
    // init
    const canvas = document.getElementById("roomCanvas");
    const ctx = canvas.getContext("2d");
    const sizeSelector = document.getElementById("canvasSizeSelect");
    const modifyNameButton = document.getElementById("modifyTableNameButton");
    const saveNameButton = document.getElementById("saveTableNameButton");
    const addTableButton = document.getElementById("addTableButton");
    const deleteTableButton = document.getElementById("deleteTableButton");
    const saveTablesButton = document.getElementById("saveButton");

    document.getElementById('rotateTableButton').addEventListener('click', rotateTable);


    const saveCanvasButton = document.getElementById("saveCanvasButton");

    let tables = [];
    let draggingTable = null; // dragging
    let selectedTable = null; // selected






    // draw tables
    function drawTables() {
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        tables.forEach(table => {
            ctx.save();
            ctx.translate(table.x + table.width / 2, table.y + table.height / 2);

            if (table.rotated) {
                ctx.rotate(Math.PI / 2);
            }

            const radius = 10; // rounded corners
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

            ctx.fillStyle ="#6D9F71";

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

            ctx.restore();
        });
    }

    // load tables
    function loadTables() {
        fetch('/api/tables')
            .then(response => response.json())
            .then(data => {
                tables = data.map(table => ({
                    ...table,
                    selected: false,
                }));
                drawTables();
            })
            .catch(error => console.error("Erreur lors du chargement des tables :", error));
    }

    // resize canvas
    function resizeCanvas(scale) {
        const originalWidth = 800;
        canvas.width = originalWidth * scale;
        canvas.height = 600;
        drawTables();
    }

    // scale select
    sizeSelector.addEventListener("change", (event) => {
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

    // add table
    addTableButton.addEventListener("click", () => {
        const seats = parseInt(document.getElementById("seatsInput").value);
        if (seats < 2 || seats > 10 || seats % 2 !== 0) {
            alert("Veuillez entrer un nombre valide (2, 4, 6, 8 ou 10).");
            return;
        }
        const newTable = {
            id: tables.length + 1,
            name: `Table ${tables.length + 1}`,
            x: 100,
            y: 100,
            width: 50 * (seats / 2),
            height: 50,
            seats: seats,
            rotated: false,
            selected: false,
        };
        tables.push(newTable);
        drawTables();
    });


    // rotate table
    function rotateTable() {
        if (!selectedTable) {
            alert('Veuillez sélectionner une table pour la faire pivoter.');
            return;
        }
        selectedTable.rotated = !selectedTable.rotated;
        drawTables();
    }

    // delete table
    deleteTableButton.addEventListener("click", () => {
        if (!selectedTable) {
            alert("Veuillez sélectionner une table à supprimer.");
            return;
        }
        tables = tables.filter(table => table !== selectedTable);
        selectedTable = null;
        drawTables();
    });

    // save tables
    saveTablesButton.addEventListener("click", () => {
        const dataToSend = tables.map(({ x, y, width, height, seats, rotated }) => ({
            x: Math.round(x),
            y: Math.round(y),
            width: Math.round(width),
            height: Math.round(height),
            seats: seats,
            rotated: rotated,
            name: "Table",
        }));

        fetch('/api/tables', {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(dataToSend),
        })
            .then(response => {
                if (response.ok) alert("Tables enregistrées avec succès !");
                else throw new Error("Erreur lors de l'enregistrement.");
            })
            .catch(error => alert(`Erreur : ${error.message}`));
    });

    // load canvas size
    async function loadCanvasSize() {
        try {
            const response = await fetch('/api/canvas/get');
            const data = await response.json();
            if (data.width && data.height) {
                canvas.width = data.width;
                canvas.height = data.height;
                return { width: data.width, height: data.height };
            }
            throw new Error("La taille du canvas n'est pas disponible.");
        } catch (error) {
            console.error("Erreur lors du chargement de la taille du canvas :", error);
            return { width: 800, height: 600 };
        }
    }

    // save canvas size
    saveCanvasButton.addEventListener("click", () => {
        const dataToSend = {
            width: canvas.width,
            height: canvas.height,
        };

        fetch('/api/canvas/save', {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(dataToSend),
        })
            .then(response => {
                if (response.ok) {
                    alert("Dimensions du canvas enregistrées avec succès !");
                } else {
                    throw new Error("Erreur lors de l'enregistrement des dimensions.");
                }
            })
            .catch(error => {
                console.error("Erreur :", error.message);
                alert(`Erreur : ${error.message}`);
            });
    });

    // select table
    canvas.addEventListener("mousedown", (e) => {
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

    canvas.addEventListener("mousemove", (e) => {
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

    // open rename modal
    modifyNameButton.addEventListener("click", () => {
        if (!selectedTable) {
            alert("Veuillez sélectionner une table en cliquant dessus.");
            return;
        }

        document.getElementById("tableNameInput").value = selectedTable.name || "";

        const tableNameModal = new bootstrap.Modal(document.getElementById("tableNameModal"));
        tableNameModal.show();
    });

    saveNameButton.addEventListener("click", () => {
        const newName = document.getElementById("tableNameInput").value.trim();

        if (!newName) {
            alert("Veuillez entrer un nouveau nom pour la table.");
            return;
        }

        if (selectedTable) {
            selectedTable.name = newName;

            fetch(`/api/tables/${selectedTable.id}`, {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({ name: newName }),
            })


                .then(response => {
                    if (response.ok) {
                        drawTables();
                        alert(`Le nom de la table a été mis à jour en "${newName}".`);
                        const tableNameModal = bootstrap.Modal.getInstance(document.getElementById("tableNameModal"));
                        tableNameModal.hide();
                    } else {
                        return response.json().then(error => {
                            throw new Error(error.message || "Erreur lors de la mise à jour.");
                        });
                    }
                })
                .catch(error => {
                    console.error("Erreur :", error);
                    alert(`Erreur lors de la sauvegarde dans la base de données : ${error.message}`);
                });
        }
    });

    // init
    const defaultCanvasSize = await loadCanvasSize();
    canvas.width = defaultCanvasSize.width;
    canvas.height = defaultCanvasSize.height;
    loadTables();





});
