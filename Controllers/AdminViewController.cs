using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Table_Reservation.Data;
using Table_Reservation.Models;
using Table_Reservation.Models.ViewModels;
using Table_Reservation.Services;

// Auth JWT Admin
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class AdminViewController : Controller
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;

    public AdminViewController(AppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    [HttpGet]
    public IActionResult Dashboard()
    {
        return View("~/Views/Admin/Dashboard.cshtml");
    }

    [HttpGet]
    public IActionResult Upload()
    {
        var isDutch = (CultureInfo.CurrentUICulture?.TwoLetterISOLanguageName?.ToLowerInvariant() ?? "fr") == "nl";
        return View(isDutch ? "~/Views/Admin/Upload.nl.cshtml" : "~/Views/Admin/Upload.cshtml");
    }

    [HttpGet]
    public async Task<IActionResult> Reservations(int offset = 0)
    {
        var referenceDate = DateTime.Today.AddMonths(offset);
        var firstOfMonth = new DateTime(referenceDate.Year, referenceDate.Month, 1);
        var lastOfMonth = firstOfMonth.AddMonths(1).AddDays(-1);

        int shiftToMonday = ((int)firstOfMonth.DayOfWeek + 6) % 7;
        var calendarStart = firstOfMonth.AddDays(-shiftToMonday);
        int shiftFromSunday = 6 - (((int)lastOfMonth.DayOfWeek + 6) % 7);
        var calendarEnd = lastOfMonth.AddDays(shiftFromSunday);

        var reservationsInRange = await _context.Reservations
            .Where(r => r.ReservationHoure >= calendarStart && r.ReservationHoure <= calendarEnd.AddDays(1))
            .OrderBy(r => r.ReservationHoure)
            .ThenBy(r => r.IsCancelled)
            .Select(r => new AdminReservationSummaryViewModel
            {
                Id = r.Id,
                TableName = r.TableName,
                Start = r.ReservationHoure,
                ClientName = r.ClientName,
                ClientEmail = r.ClientEmail,
                ClientPhone = r.ClientPhone,
                Amount = r.Amount,
                IsCancelled = r.IsCancelled,
                CancelledAt = r.CancelledAt,
                ClientProfileDeleted = r.ClientId == null
            })
            .ToListAsync();

        var reservationsByDate = reservationsInRange
            .GroupBy(r => r.Start.Date)
            .ToDictionary(g => g.Key, g => (IList<AdminReservationSummaryViewModel>)g.ToList());

        var isDutch = (CultureInfo.CurrentUICulture?.TwoLetterISOLanguageName?.ToLowerInvariant() ?? "fr") == "nl";

        var weeks = new List<IList<AdminReservationCalendarDayViewModel>>();
        var current = calendarStart;

        while (current <= calendarEnd)
        {
            var week = new List<AdminReservationCalendarDayViewModel>();
            for (int i = 0; i < 7; i++)
            {
                reservationsByDate.TryGetValue(current.Date, out var dayReservations);
                week.Add(new AdminReservationCalendarDayViewModel
                {
                    Date = current,
                    IsInCurrentMonth = current.Month == referenceDate.Month,
                    IsToday = current.Date == DateTime.Today,
                    Reservations = dayReservations ?? new List<AdminReservationSummaryViewModel>()
                });
                current = current.AddDays(1);
            }
            weeks.Add(week);
        }

        var viewModel = new AdminReservationCalendarViewModel
        {
            Weeks = weeks,
            MonthReference = firstOfMonth,
            MonthOffset = offset,
            IsDutch = isDutch
        };

        return View("~/Views/Admin/Reservations.cshtml", viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Clients(string? q)
    {
        var isDutch = (CultureInfo.CurrentUICulture?.TwoLetterISOLanguageName?.ToLowerInvariant() ?? "fr") == "nl";
        var normalizedQuery = q?.Trim().ToLowerInvariant();

        var clientsQuery = _context.ClientModels.AsQueryable();

        if (!string.IsNullOrEmpty(normalizedQuery))
        {
            var likeQuery = $"%{normalizedQuery}%";
            clientsQuery = clientsQuery.Where(c =>
                EF.Functions.Like(c.ClientName, likeQuery) ||
                EF.Functions.Like(c.ClientEmail, likeQuery) ||
                EF.Functions.Like(c.ClientPhone, likeQuery));
        }

        var clients = await clientsQuery
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new AdminClientListItem
            {
                Id = c.Id,
                Name = c.ClientName,
                Email = c.ClientEmail,
                Phone = c.ClientPhone,
                ReservationsCount = c.Reservations.Count,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync();

        var viewModel = new AdminClientsViewModel
        {
            Clients = clients,
            Query = q ?? string.Empty,
            IsDutch = isDutch
        };

        return View("~/Views/Admin/Clients.cshtml", viewModel);
    }

    [HttpGet]
    public IActionResult Newsletter()
    {
        var isDutch = (CultureInfo.CurrentUICulture?.TwoLetterISOLanguageName?.ToLowerInvariant() ?? "fr") == "nl";
        return View(isDutch ? "~/Views/Admin/Newsletter.nl.cshtml" : "~/Views/Admin/Newsletter.cshtml");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendNewsletter(string subject, string content, string campaignType, string? couponCode, decimal? discountPercentage, DateTime? validUntil)
    {
        try
        {
            var isDutch = (CultureInfo.CurrentUICulture?.TwoLetterISOLanguageName?.ToLowerInvariant() ?? "fr") == "nl";
            
            // Récupérer tous les emails des clients
            var clientEmails = await _context.ClientModels
                .Where(c => !string.IsNullOrEmpty(c.ClientEmail))
                .Select(c => c.ClientEmail)
                .Distinct()
                .ToListAsync();

            if (!clientEmails.Any())
            {
                TempData["NewsletterError"] = isDutch 
                    ? "Geen e-mailadressen gevonden om naar te sturen." 
                    : "Aucune adresse email trouvée pour l'envoi.";
                return RedirectToAction("Newsletter");
            }

            // Créer la campagne
            var campaign = new NewsletterCampaign
            {
                Subject = subject,
                Content = content,
                CampaignType = campaignType ?? "Newsletter",
                CouponCode = couponCode,
                DiscountPercentage = discountPercentage,
                ValidUntil = validUntil,
                TotalRecipients = clientEmails.Count,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = User.Identity?.Name ?? "Admin"
            };

            _context.NewsletterCampaigns.Add(campaign);
            await _context.SaveChangesAsync();

            // Envoyer les emails
            int sentCount = 0;
            int failedCount = 0;

            foreach (var email in clientEmails)
            {
                try
                {
                    // Ajouter les informations de coupon si c'est une promotion
                    var emailContent = content;
                    if (campaignType == "Promotion" && !string.IsNullOrEmpty(couponCode))
                    {
                        var couponInfo = isDutch
                            ? $"<div style='background-color: #f0f0f0; padding: 20px; margin: 20px 0; border-radius: 5px; text-align: center;'><h3 style='color: #d4af37;'>Promotiecode: <strong>{couponCode}</strong></h3>"
                            : $"<div style='background-color: #f0f0f0; padding: 20px; margin: 20px 0; border-radius: 5px; text-align: center;'><h3 style='color: #d4af37;'>Code promo: <strong>{couponCode}</strong></h3>";
                        
                        if (discountPercentage.HasValue)
                        {
                            couponInfo += isDutch
                                ? $"<p style='font-size: 18px;'><strong>{discountPercentage.Value}% korting</strong></p>"
                                : $"<p style='font-size: 18px;'><strong>{discountPercentage.Value}% de réduction</strong></p>";
                        }
                        
                        if (validUntil.HasValue)
                        {
                            couponInfo += isDutch
                                ? $"<p>Geldig tot: {validUntil.Value:dd/MM/yyyy}</p>"
                                : $"<p>Valable jusqu'au: {validUntil.Value:dd/MM/yyyy}</p>";
                        }
                        
                        couponInfo += "</div>";
                        emailContent = couponInfo + emailContent;
                    }

                    await _emailService.SendNewsletterEmailAsync(email, subject, emailContent, isDutch);
                    sentCount++;
                }
                catch (Exception ex)
                {
                    failedCount++;
                    // Log l'erreur mais continue avec les autres emails
                    Console.WriteLine($"Erreur lors de l'envoi à {email}: {ex.Message}");
                }
            }

            // Mettre à jour la campagne
            campaign.SentCount = sentCount;
            campaign.FailedCount = failedCount;
            campaign.SentAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["NewsletterSuccess"] = isDutch
                ? $"Newsletter succesvol verzonden naar {sentCount} ontvanger(s). {failedCount} mislukt."
                : $"Newsletter envoyée avec succès à {sentCount} destinataire(s). {failedCount} échec(s).";

            return RedirectToAction("Newsletter");
        }
        catch (Exception ex)
        {
            var isDutch = (CultureInfo.CurrentUICulture?.TwoLetterISOLanguageName?.ToLowerInvariant() ?? "fr") == "nl";
            TempData["NewsletterError"] = isDutch
                ? $"Erreur lors de l'envoi: {ex.Message}"
                : $"Erreur lors de l'envoi: {ex.Message}";
            return RedirectToAction("Newsletter");
        }
    }
}
