using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Stripe;
using Stripe.Checkout;
<<<<<<< HEAD
using Table_Reservation.Services;
=======
>>>>>>> old-origin/master

namespace Table_Reservation.Controllers
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : Controller
    {
        private readonly string _stripeSecretKey;
        private readonly string _webhookSecret;
        private readonly AppDbContext _context;
<<<<<<< HEAD
        private readonly IEmailService _emailService;
        private readonly SmsService _smsService;  

        public PaymentController(IConfiguration configuration, AppDbContext context, IEmailService emailService, SmsService smsService)
=======

        public PaymentController(IConfiguration configuration, AppDbContext context)
>>>>>>> old-origin/master
        {
            _stripeSecretKey = configuration["Stripe:SecretKey"];
            _webhookSecret = configuration["Stripe:WebhookSecret"];
            _context = context;
<<<<<<< HEAD
            _emailService = emailService;
            _smsService = smsService;  
=======
>>>>>>> old-origin/master

            if (string.IsNullOrEmpty(_stripeSecretKey))
            {
                throw new Exception("La clé API Stripe n'est pas configurée.");
            }

            StripeConfiguration.ApiKey = _stripeSecretKey;
        }

        [HttpPost("create-session")]
        public IActionResult CreateSession([FromBody] PaymentRequest request)
        {
<<<<<<< HEAD
            try
            {
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card", "bancontact" },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                Currency = "eur",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = $"Réservation {request.TableName}",
                                },
                                UnitAmount = 100, 
                            },
                            Quantity = 1,
                        },
                    },
                    Mode = "payment",
                    SuccessUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/api/payment/success?session_id={{CHECKOUT_SESSION_ID}}",
                    CancelUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/api/payment/cancel",
                    Metadata = new Dictionary<string, string>
                    {
                        { "tableId", request.TableId.ToString() },
                        { "TableName", request.TableName },
                        { "Date", request.Date},
                        { "StartTime", request.StartTime },
                        { "clientName", request.ClientName },
                        { "clientEmail", request.ClientEmail },
                        { "clientPhone", request.ClientPhone }
                    }
                };

                var service = new SessionService();
                Session session = service.Create(options);

                return Ok(new { sessionUrl = session.Url });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la création de la session : {ex.Message}");
                return StatusCode(500, "Erreur interne du serveur.");
            }
=======
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "eur",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                        Name = $"Réservation {request.TableName}",

        },
                            UnitAmount = 100,
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = $"https://localhost:44340/api/payment/success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = "https://localhost:44340/api/payment/cancel",
                Metadata = new Dictionary<string, string>
                {
                    { "tableId", request.TableId.ToString() },
                    { "TableName", request.TableName },
                    { "clientName", request.ClientName },
                    { "clientEmail", request.ClientEmail },
                    { "clientPhone", request.ClientPhone }
                }
            };

            var service = new SessionService();
            Session session = service.Create(options);

            return Ok(new { sessionUrl = session.Url });
>>>>>>> old-origin/master
        }

        [HttpPost("webhook")]
        public IActionResult StripeWebhook()
        {
            var json = new StreamReader(HttpContext.Request.Body).ReadToEnd();
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _webhookSecret
                );

                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Session;

<<<<<<< HEAD
                    if (session == null || session.Metadata.Count == 0)
                    {
                        Console.WriteLine("Session Stripe invalide ou sans métadonnées.");
                        return BadRequest();
                    }

                    if (!_context.Reservations.Any(r => r.ClientEmail == session.Metadata["clientEmail"] && r.ReservationDate == DateTime.Parse(session.Metadata["Date"]).Date))
                    {
                        var reservation = new ReservationModel
                        {
                            TableId = int.Parse(session.Metadata["tableId"]),
                            TableName = session.Metadata["TableName"],
                            ClientName = session.Metadata["clientName"],
                            ClientEmail = session.Metadata["clientEmail"],
                            ClientPhone = session.Metadata["clientPhone"],
                            ReservationDate = DateTime.Parse(session.Metadata["Date"]).Date,
                            ReservationHoure = DateTime.Parse(session.Metadata["StartTime"]).Date.Add(DateTime.Parse(session.Metadata["StartTime"]).TimeOfDay),
                            Amount = (int)session.AmountTotal / 100
                        };

                        _context.Reservations.Add(reservation);
                        _context.SaveChanges();
                    }
=======
                    // Enregistrer la réservation dans la base de données
                    var reservation = new ReservationModel
                    {
                        TableId = int.Parse(session.Metadata["tableId"]),
                        TableName = session.Metadata["TableName"],
                        ClientName = session.Metadata["clientName"],
                        ClientEmail = session.Metadata["clientEmail"],
                        ClientPhone = session.Metadata["clientPhone"],
                        ReservationDate = DateTime.Now, // Exemple
                        ReservationHoure = DateTime.Now, // Exemple
                        Amount = (int)session.AmountTotal / 100
                    };

                    // Validation des dates avant sauvegarde
                    if (reservation.ReservationDate == default || reservation.ReservationDate < new DateTime(1753, 1, 1))
                    {
                        reservation.ReservationDate = DateTime.Now; // Définir une date valide par défaut
                    }

                    if (reservation.ReservationHoure == default || reservation.ReservationHoure < new DateTime(1753, 1, 1))
                    {
                        reservation.ReservationHoure = DateTime.Now; // Définir une heure valide par défaut
                    }

                    _context.Reservations.Add(reservation);
                    _context.SaveChanges();
>>>>>>> old-origin/master
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur Stripe Webhook : {ex.Message}");
                return BadRequest();
            }
        }

<<<<<<< HEAD
        [HttpGet("success")]
        public async Task<IActionResult> Success(string session_id)
=======

        [HttpGet("success")]
        public IActionResult Success(string session_id)
>>>>>>> old-origin/master
        {
            if (string.IsNullOrEmpty(session_id))
            {
                return BadRequest("Session ID manquant.");
            }

            var service = new SessionService();
            var session = service.Get(session_id);

<<<<<<< HEAD
            if (session == null || session.Metadata.Count == 0)
            {
                return BadRequest("Session Stripe invalide.");
            }

            if (!_context.Reservations.Any(r => r.ClientEmail == session.Metadata["clientEmail"] && r.ReservationDate == DateTime.Parse(session.Metadata["Date"]).Date))
            {
                var reservation = new ReservationModel
                {
                    TableId = int.Parse(session.Metadata["tableId"]),
                    TableName = session.Metadata["TableName"],
                    ClientName = session.Metadata["clientName"],
                    ClientEmail = session.Metadata["clientEmail"],
                    ClientPhone = session.Metadata["clientPhone"],
                    ReservationDate = DateTime.Parse(session.Metadata["Date"]).Date,
                    ReservationHoure = DateTime.Parse(session.Metadata["StartTime"]).Date.Add(DateTime.Parse(session.Metadata["StartTime"]).TimeOfDay),
                    Amount = (int)session.AmountTotal / 100
                };

                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();

                await _emailService.SendReservationConfirmationEmailAsync(reservation.ClientEmail, reservation);

                return View("~/Views/Payment/Success.cshtml", reservation);
            }

            return Redirect("/reservation-success");
=======
            // Initialiser une réservation avec validation des dates
            var reservation = new ReservationModel
            {
                TableId = int.Parse(session.Metadata["tableId"]),
                TableName = session.Metadata["TableName"],
                ClientName = session.Metadata["clientName"],
                ClientEmail = session.Metadata["clientEmail"],
                ClientPhone = session.Metadata["clientPhone"],
                ReservationDate = DateTime.Now, // Exemple
                ReservationHoure = DateTime.Now, // Exemple
                Amount = (int)session.AmountTotal / 100
            };

            // Validation des dates avant sauvegarde
            if (reservation.ReservationDate == default || reservation.ReservationDate < new DateTime(1753, 1, 1))
            {
                reservation.ReservationDate = DateTime.Now; // Définir une date valide par défaut
            }

            if (reservation.ReservationHoure == default || reservation.ReservationHoure < new DateTime(1753, 1, 1))
            {
                reservation.ReservationHoure = DateTime.Now; // Définir une heure valide par défaut
            }

            // Ajouter à la base de données
            try
            {
                _context.Reservations.Add(reservation);
                _context.SaveChanges();

                return View("~/Views/Payment/Success.cshtml", reservation);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'enregistrement dans la base de données : {ex.Message}");
                return StatusCode(500, "Erreur lors de l'enregistrement.");
            }
>>>>>>> old-origin/master
        }

        [HttpGet("cancel")]
        public IActionResult Cancel()
        {
            return View("~/Views/Payment/Cancel.cshtml");
        }
    }

    public class PaymentRequest
    {
        public int TableId { get; set; }
        public string ClientName { get; set; }
<<<<<<< HEAD
        public string TableName { get; set; }
        public string Date { get; set; }
        public string StartTime { get; set; }
=======
        public string TableName { get; set; } // Nouveau champ pour le nom de la table

>>>>>>> old-origin/master
        public string ClientEmail { get; set; }
        public string ClientPhone { get; set; }
    }
}
