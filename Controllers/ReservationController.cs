using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Globalization;
using Table_Reservation.Data; // Pour accéder au DbContext
using Table_Reservation.Models;
using Table_Reservation.Services;

public class ReservationController : Controller
{
    private readonly AppDbContext _context;
    private readonly ClientAccountService _clientAccountService;

    public ReservationController(AppDbContext context, ClientAccountService clientAccountService)
    {
        _context = context;
        _clientAccountService = clientAccountService;
    }

    public async Task<IActionResult> Success(string sessionId)
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
            var reservationDate = DateTime.Parse(session.Metadata["Date"], CultureInfo.InvariantCulture).Date;
            var reservationStart = ClientAccountService.CombineDateAndTime(reservationDate, session.Metadata["StartTime"]);
            var tableId = int.Parse(session.Metadata["tableId"], CultureInfo.InvariantCulture);
            var amountTotal = session.AmountTotal ?? 0;

            if (!await _clientAccountService.ReservationExistsAsync(tableId, session.Metadata["clientEmail"], reservationDate, reservationStart))
            {
                var reservation = await _clientAccountService.CreateReservationAsync(
                    tableId,
                    session.Metadata["TableName"],
                    session.Metadata["clientName"],
                    session.Metadata["clientEmail"],
                    session.Metadata["clientPhone"],
                    reservationDate,
                    reservationStart,
                    amountTotal);

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
                await _context.SaveChangesAsync();

                Console.WriteLine("Enregistrement réussi dans la base de données !");

                // Retourner la vue avec la réservation
                return View("~/Views/Payment/Success.cshtml", reservation);
            }

            return Redirect("/reservation-success");
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

    [HttpGet]
    [Route("api/reservations")]
    public IActionResult GetReservations([FromQuery] string date, [FromQuery] string time)
    {
        if (string.IsNullOrEmpty(date) || string.IsNullOrEmpty(time))
        {
            return BadRequest("Les paramètres 'date' et 'time' sont requis.");
        }

        try
        {
            // Convertir les paramètres en objets DateTime/TimeSpan
            DateTime reservationDate = DateTime.Parse(date);
            TimeSpan reservationTime = TimeSpan.Parse(time);

            // Récupérer toutes les réservations pour la date donnée
            var reservations = _context.Reservations
                .Where(r => r.ReservationDate == reservationDate && !r.IsCancelled)
                .ToList(); // Récupération des données côté mémoire

            // Filtrer en mémoire pour gérer la logique de TimeSpan // bug 2h apres et une 1 avant pour bloquer tables
            var filteredReservations = reservations
                 .Where(r =>
                     r.ReservationHoure.TimeOfDay.Subtract(TimeSpan.FromHours(1)) <= reservationTime && // L'heure choisie est après ou égale à (heure de début - 1 heures)
                     r.ReservationHoure.TimeOfDay.Add(TimeSpan.FromHours(2)) > reservationTime // L'heure choisie est avant (heure de début + 2 heures)
                 )
                 .Select(r => new
                 {
                     r.TableId,
                     r.ReservationDate,
                     r.ReservationHoure
                 })
                 .ToList();

            Console.WriteLine($"Réservations filtrées : {string.Join(", ", filteredReservations.Select(r => r.TableId))}");

            return Ok(filteredReservations); // Retourner les résultats filtrés
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur : {ex.Message}");
            return StatusCode(500, "Erreur lors de la récupération des réservations.");
        }
    }


    private dynamic GetPaymentDetails(string sessionId)
    {
        // Méthode fictive pour récupérer les détails du paiement via Stripe API
        // Implémentez une logique pour appeler Stripe et récupérer les détails
        throw new NotImplementedException();
    }



}
