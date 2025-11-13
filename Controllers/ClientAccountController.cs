using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using Table_Reservation.Data;
using Table_Reservation.Models.ViewModels;
using Table_Reservation.Services;

namespace Table_Reservation.Controllers
{
    public class ClientAccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public ClientAccountController(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> Reservations()
        {
            var clientId = HttpContext.Session.GetInt32("ClientId");
            if (!clientId.HasValue)
            {
                return RedirectToAction("Index", "Home");
            }

            var client = await _context.ClientModels
                .Include(c => c.Reservations)
                .SingleOrDefaultAsync(c => c.Id == clientId.Value);

            if (client == null)
            {
                HttpContext.Session.Remove("ClientId");
                HttpContext.Session.Remove("ClientName");
                HttpContext.Session.Remove("ClientEmail");
                return RedirectToAction("Index", "Home");
            }

            var nowLocal = DateTime.Now;
            var reservations = client.Reservations
                .OrderByDescending(r => r.ReservationDate)
                .ThenByDescending(r => r.ReservationHoure)
                .Select(r =>
                {
                    var reservationStart = DateTime.SpecifyKind(r.ReservationHoure, DateTimeKind.Local);
                    var canCancel = !r.IsCancelled && reservationStart - nowLocal > TimeSpan.FromHours(24);
                    return new ClientReservationListItem
                    {
                        Id = r.Id,
                        TableName = r.TableName,
                        ReservationDate = r.ReservationDate,
                        ReservationStart = reservationStart,
                        Amount = r.Amount,
                        IsCancelled = r.IsCancelled,
                        CanCancel = canCancel
                    };
                })
                .ToList();

            var viewModel = new ClientDashboardViewModel
            {
                ClientName = client.ClientName,
                ClientEmail = client.ClientEmail,
                Reservations = reservations
            };

            return View("~/Views/ClientAccount/Reservations.cshtml", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelReservation(int id)
        {
            var clientId = HttpContext.Session.GetInt32("ClientId");
            if (!clientId.HasValue)
            {
                return RedirectToAction(nameof(Reservations));
            }

            var reservation = await _context.Reservations
                .SingleOrDefaultAsync(r => r.Id == id && r.ClientId == clientId.Value);

            if (reservation == null)
            {
                TempData["ClientDashboardError"] = Translate("Réservation introuvable.", "Reservering niet gevonden.");
                return RedirectToAction(nameof(Reservations));
            }

            var nowLocal = DateTime.Now;
            var reservationStartLocal = DateTime.SpecifyKind(reservation.ReservationHoure, DateTimeKind.Local);

            if (reservation.IsCancelled)
            {
                TempData["ClientDashboardInfo"] = Translate("Cette réservation est déjà annulée.", "Deze reservering is al geannuleerd.");
                return RedirectToAction(nameof(Reservations));
            }

            if (reservationStartLocal <= nowLocal.AddHours(24))
            {
                TempData["ClientDashboardError"] = Translate("Vous ne pouvez annuler qu'une réservation prévue dans plus de 24 heures.", "Je kan enkel annuleren als de reservering over meer dan 24 uur plaatsvindt.");
                return RedirectToAction(nameof(Reservations));
            }

            reservation.IsCancelled = true;
            reservation.CancelledAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            try
            {
                await _emailService.SendReservationCancellationEmailAsync(
                    reservation.ClientEmail,
                    reservation,
                    IsDutchCulture());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'envoi de l'email d'annulation : {ex.Message}");
            }

            TempData["ClientDashboardSuccess"] = Translate("Votre réservation a été annulée.", "Je reservering werd geannuleerd.");
            return RedirectToAction(nameof(Reservations));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProfile()
        {
            var clientId = HttpContext.Session.GetInt32("ClientId");
            if (!clientId.HasValue)
            {
                return RedirectToAction("Index", "Home");
            }

            var client = await _context.ClientModels
                .Include(c => c.Reservations)
                .SingleOrDefaultAsync(c => c.Id == clientId.Value);

            if (client == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Index", "Home");
            }

            foreach (var reservation in client.Reservations)
            {
                reservation.ClientId = null;
                reservation.Client = null;
            }

            _context.ClientModels.Remove(client);
            await _context.SaveChangesAsync();

            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        private static bool IsDutchCulture()
        {
            var culture = CultureInfo.CurrentUICulture?.TwoLetterISOLanguageName;
            return string.Equals(culture, "nl", StringComparison.OrdinalIgnoreCase);
        }

        private static string Translate(string fr, string nl) => IsDutchCulture() ? nl : fr;
    }
}

