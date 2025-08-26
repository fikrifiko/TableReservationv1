﻿using Microsoft.AspNetCore.Mvc;
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

<<<<<<< HEAD
=======
    // Action pour afficher la page de réservation client
    public IActionResult Index()
    {
        return View("PageReservationClient"); // Retourne la vue Page_Reservation_Client.cshtml
    }

>>>>>>> old-origin/master
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
<<<<<<< HEAD
                TableName = session.Metadata["TableName"],
=======
                                TableName = session.Metadata["TableName"],
>>>>>>> old-origin/master

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

<<<<<<< HEAD
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
                .Where(r => r.ReservationDate == reservationDate)
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

=======
>>>>>>> old-origin/master

    private dynamic GetPaymentDetails(string sessionId)
    {
        // Méthode fictive pour récupérer les détails du paiement via Stripe API
        // Implémentez une logique pour appeler Stripe et récupérer les détails
        throw new NotImplementedException();
    }


<<<<<<< HEAD
=======
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


>>>>>>> old-origin/master

}
