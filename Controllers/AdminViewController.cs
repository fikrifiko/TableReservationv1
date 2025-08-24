using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
// Auth JWT Admin
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class AdminViewController : Controller
{

    [HttpGet]
    public IActionResult Dashboard()
    {
        return View("~/Views/Admin/Dashboard.cshtml");
    }

    [HttpGet]
    public IActionResult Disposition()
    {
        return View("~/Views/Admin/Disposition.cshtml");
    }
    [HttpGet]
    public IActionResult Upload()
    {
        return View("~/Views/Admin/Upload.cshtml");
    }
}
