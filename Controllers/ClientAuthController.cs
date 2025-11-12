using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using Table_Reservation.Data;
using Table_Reservation.Models;
using BCryptNet = BCrypt.Net.BCrypt;

namespace Table_Reservation.Controllers
{
    [ApiController]
    [Route("api/client")]
    public class ClientAuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClientAuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] ClientLoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = Translate("Données de connexion invalides.", "Ongeldige inloggegevens.") });
            }

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var client = await _context.ClientModels
                .SingleOrDefaultAsync(c => c.ClientEmail == normalizedEmail);

            if (client == null)
            {
                return Unauthorized(new { message = Translate("Email ou mot de passe incorrect.", "Ongeldig e-mailadres of wachtwoord.") });
            }

            var password = request.Password.Trim();
            var passwordMatches = false;
            var passwordNeedsUpgrade = false;
            try
            {
                passwordMatches = BCryptNet.Verify(password, client.PasswordHash);
            }
            catch
            {
                if (string.Equals(client.PasswordHash, password, StringComparison.Ordinal) ||
                    string.Equals(client.PasswordHash, client.ClientPhone, StringComparison.Ordinal))
                {
                    passwordMatches = true;
                    passwordNeedsUpgrade = true;
                }
            }

            if (!passwordMatches)
            {
                return Unauthorized(new { message = Translate("Email ou mot de passe incorrect.", "Ongeldig e-mailadres of wachtwoord.") });
            }

            if (passwordNeedsUpgrade)
            {
                client.PasswordHash = BCryptNet.HashPassword(password);
                client.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            HttpContext.Session.SetInt32("ClientId", client.Id);
            HttpContext.Session.SetString("ClientName", client.ClientName);
            HttpContext.Session.SetString("ClientEmail", client.ClientEmail);

            return Ok(new { success = true, name = client.ClientName });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("ClientId");
            HttpContext.Session.Remove("ClientName");
            HttpContext.Session.Remove("ClientEmail");

            return Ok(new { success = true, message = Translate("Déconnexion réussie.", "Succesvol uitgelogd.") });
        }

        [HttpGet("session")]
        public IActionResult SessionInfo()
        {
            var clientId = HttpContext.Session.GetInt32("ClientId");
            if (!clientId.HasValue)
            {
                return Unauthorized(new { message = Translate("Utilisateur non connecté.", "Gebruiker niet aangemeld.") });
            }

            return Ok(new
            {
                id = clientId.Value,
                name = HttpContext.Session.GetString("ClientName") ?? string.Empty,
                email = HttpContext.Session.GetString("ClientEmail") ?? string.Empty
            });
        }

        private static bool IsDutchCulture()
        {
            var culture = CultureInfo.CurrentUICulture?.TwoLetterISOLanguageName;
            return string.Equals(culture, "nl", StringComparison.OrdinalIgnoreCase);
        }

        private static string Translate(string fr, string nl) => IsDutchCulture() ? nl : fr;
    }
}

