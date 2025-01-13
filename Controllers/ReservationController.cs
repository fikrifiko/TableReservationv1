using Microsoft.AspNetCore.Mvc;
using System;
using Table_Reservation.Data; // Pour accéder au DbContext
using Table_Reservation.Models;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System;

public class ReservationController : Controller
{
    private readonly AppDbContext _context;

    public ReservationController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Success(string sessionId)
    {
        try
        {
            // Vérifier si le sessionId est valide
            if (string.IsNullOrEmpty(sessionId))
            {
                Console.WriteLine("Erreur : sessionId est manquant.");
                return BadRequest("Session ID manquant.");
            }

            // Récupérer les informations de la session Stripe
            var service = new SessionService();
            var session = service.Get(sessionId);

            if (session == null)
            {
                Console.WriteLine("Erreur : session introuvable.");
                return BadRequest("Session introuvable.");
            }

            // Initialiser la réservation avec les métadonnées Stripe
            var reservation = new ReservationModel
            {
                TableId = int.Parse(session.Metadata["tableId"]),
                ClientName = session.Metadata["clientName"],
                ClientEmail = session.Metadata["clientEmail"],
                ClientPhone = session.Metadata["clientPhone"],
                ReservationDate = DateTime.Now, // Ajustez si nécessaire
                ReservationHoure = DateTime.Now, // Ajustez si nécessaire
                Amount = (int)(session.AmountTotal / 100) // Stripe utilise des centimes
            };

            // Afficher les données dans la console pour vérification
            Console.WriteLine("Début de l'enregistrement dans la base de données...");
            Console.WriteLine($"TableId : {reservation.TableId}");
            Console.WriteLine($"ClientName : {reservation.ClientName}");
            Console.WriteLine($"ClientEmail : {reservation.ClientEmail}");
            Console.WriteLine($"ReservationDate : {reservation.ReservationDate}");
            Console.WriteLine($"ReservationHoure : {reservation.ReservationHoure}");
            Console.WriteLine($"Amount : {reservation.Amount}");

            // Ajouter la réservation à la base de données
            _context.Reservations.Add(reservation);
            _context.SaveChanges();

            Console.WriteLine("Enregistrement réussi dans la base de données !");

            // Retourner la vue avec la réservation
            return View("~/Views/Payment/Success.cshtml", reservation);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'enregistrement dans la base de données : {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Détails de l'erreur interne : {ex.InnerException.Message}");
            }

            // Afficher une vue d'erreur ou retourner une réponse d'erreur
            return StatusCode(500, "Erreur lors de l'enregistrement dans la base de données.");
        }
    }


    private dynamic GetPaymentDetails(string sessionId)
    {
        // Méthode fictive pour récupérer les détails du paiement via Stripe API
        // Implémentez une logique pour appeler Stripe et récupérer les détails
        throw new NotImplementedException();
    }


    [HttpPost]
public IActionResult CreateTestReservation([FromBody] ReservationModel reservation)
{

    if (ModelState.IsValid)
    {
            if (reservation.ReservationDate == default || reservation.ReservationDate < new DateTime(1753, 1, 1))
            {
                reservation.ReservationDate = DateTime.Now; // Ou une autre date valide par défaut
            }

            if (reservation.ReservationHoure == default || reservation.ReservationHoure < new DateTime(1753, 1, 1))
            {
                reservation.ReservationHoure = DateTime.Now; // Ou une autre heure valide par défaut
            }


            // Simulez une réservation et enregistrez-la dans la base de données
            reservation.Amount = 1000; // Montant fictif
        _context.Reservations.Add(reservation);
        _context.SaveChanges();

        // Retournez une réponse de succès
        return Ok(new { message = "Réservation simulée avec succès." });
    }

    return BadRequest("Données de réservation invalides.");
}



}
