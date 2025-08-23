using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

public class AccountController : Controller
{
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        // Supprimer l'authentification
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // Vérifier si la session est active avant de la supprimer
        if (HttpContext.Session != null)
        {
            HttpContext.Session.Clear(); // Nettoie la session après déconnexion
        }

        // Supprimer les cookies de session
        Response.Cookies.Delete(".AspNetCore.Cookies");

        // Empêcher le cache de garder l'ancienne session
        Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["Expires"] = "0";

        return RedirectToAction("Index", "Home"); // Redirige vers la page d'accueil
    }
}
