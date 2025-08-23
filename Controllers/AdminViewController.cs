using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
public class AdminViewController : Controller
{

    [HttpGet]
    public IActionResult Dashboard()
    {
        return View("~/Views/Admin/Dashboard.cshtml"); // Assurez-vous que le fichier existe
    }

    [HttpGet]
    public IActionResult Disposition()
    {
        return View("~/Views/Admin/Disposition.cshtml"); // Assurez-vous que le fichier existe
    }
    [HttpGet]
    public IActionResult Upload()
    {
        return View("~/Views/Admin/Upload.cshtml"); // Assurez-vous que le fichier existe
    }
}
