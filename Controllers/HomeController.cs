<<<<<<< HEAD
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
=======
>>>>>>> old-origin/master
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Table_Reservation.Models;

namespace Table_Reservation.Controllers
{
<<<<<<< HEAD

=======
>>>>>>> old-origin/master
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
<<<<<<< HEAD
            return View("~/Views/Home/Index.cshtml");
        }

        public IActionResult Privacy()
        {

            return View();
        }
        public IActionResult Menu()
        {
            return View();
        }
        public IActionResult Upload()
        {
            return View();
        }
=======
            return View();
        }



        public IActionResult Privacy()
        {
            return View();
        }

>>>>>>> old-origin/master
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
