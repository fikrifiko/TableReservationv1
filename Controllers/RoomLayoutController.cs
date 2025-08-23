namespace Table_Reservation.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using Table_Reservation.Models;

    public class RoomLayoutController : Controller
    {
        private static List<RoomLayout> layouts = new List<RoomLayout>();

        [HttpGet]
        public IActionResult Index()
        {
            return View(layouts); // Affiche la liste des dispositions
        }

        [HttpPost]
        public IActionResult SaveLayout([FromBody] RoomLayout layout)
        {
            layouts.Add(layout);
            return Ok(new { message = "Layout saved successfully" });
        }

        [HttpGet("{id}")]
        public IActionResult GetLayout(int id)
        {
            var layout = layouts.Find(l => l.Id == id);
            if (layout == null) return NotFound();
            return Json(layout);
        }
    }

}
