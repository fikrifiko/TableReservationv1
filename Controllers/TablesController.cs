using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Table_Reservation.Data; // Namespace de AppDbContext
using Table_Reservation.Models; // Namespace de TableModel

namespace Table_Reservation.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Route de l'API : /api/tables
    public class TablesController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Injection de dépendance pour AppDbContext
        public TablesController(AppDbContext context)
        {
            _context = context;
        }

        // Endpoint pour enregistrer les tables
        
        [HttpPost]
        public async Task<IActionResult> SaveTables([FromBody] List<TableModel> tables)
        {
            if (tables == null || !tables.Any())
            {
                return BadRequest("Aucune table à enregistrer.");
            }

            // Réinitialiser les IDs pour éviter les conflits avec la colonne d'identité
            foreach (var table in tables)
            {
                table.Id = 0; // Forcer l'insertion d'un nouvel ID généré automatiquement
            }

            // Supprimer les anciennes tables (si nécessaire)
            _context.Tables.RemoveRange(_context.Tables);

            // Ajouter les nouvelles tables
            await _context.Tables.AddRangeAsync(tables);

            // Sauvegarder dans la base de données
            await _context.SaveChangesAsync();

            return Ok("Tables enregistrées avec succès.");
        }



        // Endpoint pour récupérer les tables
        [HttpGet]
        public IActionResult GetTables()
        {
            var tables = _context.Tables.ToList(); // Récupérer les données de la base de données
            return Ok(tables);
        }

        // Endpoint pour mettre à jour les tables
        [HttpPut]
        public async Task<IActionResult> UpdateTables([FromBody] List<TableModel> tables)
        {
            if (tables == null || !tables.Any())
            {
                return BadRequest("Aucune table à mettre à jour.");
            }

            // Supprimer les anciennes tables et ajouter les nouvelles
            _context.Tables.RemoveRange(_context.Tables);
            await _context.Tables.AddRangeAsync(tables);
            await _context.SaveChangesAsync();

            return Ok("Tables mises à jour avec succès.");
        }


        // pour supprimer les tables

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTable(int id)
        {
            // Trouver la table avec l'ID spécifié
            var table = await _context.Tables.FindAsync(id);
            if (table == null)
            {
                return NotFound("Table non trouvée.");
            }

            // Supprimer la table
            _context.Tables.Remove(table);
            await _context.SaveChangesAsync();

            return Ok("Table supprimée avec succès.");
        }

    }
}
