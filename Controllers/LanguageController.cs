using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Table_Reservation.Controllers
{
    public class LanguageController : Controller
    {
        [HttpGet]
        public IActionResult Set(string culture, string? uiCulture, string returnUrl = "/")
        {
            if (string.IsNullOrWhiteSpace(culture))
            {
                culture = "fr-FR";
            }
            // Keep only fr-FR or nl (neutral)
            if (culture.StartsWith("nl", StringComparison.OrdinalIgnoreCase)) culture = "nl";
            if (!string.Equals(culture, "fr-FR", StringComparison.OrdinalIgnoreCase) && !string.Equals(culture, "nl", StringComparison.OrdinalIgnoreCase))
            {
                culture = "fr-FR";
            }
            var ui = string.IsNullOrWhiteSpace(uiCulture) ? culture : (uiCulture.StartsWith("nl", StringComparison.OrdinalIgnoreCase) ? "nl" : uiCulture);
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture, ui)),
                new CookieOptions { Path = "/", Expires = DateTimeOffset.UtcNow.AddMonths(6), IsEssential = true, SameSite = SameSiteMode.Lax, Secure = Request.IsHttps }
            );
            if (string.IsNullOrWhiteSpace(returnUrl) || !Url.IsLocalUrl(returnUrl))
            {
                returnUrl = "/";
            }
            return LocalRedirect(returnUrl);
        }
    }
}

