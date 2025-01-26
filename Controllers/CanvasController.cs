using Microsoft.AspNetCore.Mvc;
using Table_Reservation.Data;
using Table_Reservation.Models;

namespace Table_Reservation.Controllers
{
    [ApiController]
    [Route("api/canvas")]
    public class CanvasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CanvasController(AppDbContext context)
        {
            _context = context;
        }

        // API pour enregistrer la taille du canvas
        [HttpPost("save")]
        public IActionResult SaveCanvasSize([FromBody] CanvasModel canvas)
        {
            if (canvas == null) return BadRequest("Données invalides.");

            // Supprimer les anciennes dimensions pour ne conserver qu'une entrée unique
            //var existingCanvas = _context.CanvasModels.FirstOrDefault();
            //if (existingCanvas != null)
            //{
            //    _context.CanvasModels.Remove(existingCanvas);
            //}

            //// Ajouter les nouvelles dimensions
            //_context.CanvasModels.Add(canvas);
            //_context.SaveChanges();


            var canvasDB = _context.CanvasModels.FirstOrDefault();
            if (canvasDB != null) { 
            canvasDB.Width= canvas.Width;
            canvasDB.Height= canvas.Height; 
                _context.CanvasModels.Update(canvasDB);
                _context.SaveChanges();
                
            
            }

            return Ok("Taille du canvas enregistrée avec succès.");
        }

        // API pour récupérer la taille du canvas
        [HttpGet("get")]
        public IActionResult GetCanvasSize()
        {
            var canvas = _context.CanvasModels.FirstOrDefault();
            if (canvas == null) return NotFound("Aucune dimension trouvée.");

            return Ok(canvas);
        }
    }
}
