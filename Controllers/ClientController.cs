using Microsoft.AspNetCore.Mvc;

namespace Table_Reservation.Controllers
{
    public class ClientController : Controller
    {
        // Action pour afficher la page de réservation client
        public IActionResult PageReservationClient()
        {
            return View("PageReservationClient"); // Retourne la vue Page_Reservation_Client.cshtml
        }
    }
}
