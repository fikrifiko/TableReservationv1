using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace Table_Reservation.Controllers
{
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : Controller
    {
        private readonly string _stripeSecretKey;
        private readonly string _webhookSecret;
        private readonly AppDbContext _context;

        public PaymentController(IConfiguration configuration, AppDbContext context)
        {
            _stripeSecretKey = configuration["Stripe:SecretKey"];
            _webhookSecret = configuration["Stripe:WebhookSecret"];
            _context = context;

            if (string.IsNullOrEmpty(_stripeSecretKey))
            {
                throw new Exception("La clé API Stripe n'est pas configurée.");
            }

            StripeConfiguration.ApiKey = _stripeSecretKey;
        }

        [HttpPost("create-session")]
        public IActionResult CreateSession([FromBody] PaymentRequest request)
        {
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
                                Name = $"Réservation Table {request.TableId}",
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
                    { "clientName", request.ClientName },
                    { "clientEmail", request.ClientEmail },
                    { "clientPhone", request.ClientPhone }
                }
            };

            var service = new SessionService();
            Session session = service.Create(options);

            return Ok(new { sessionUrl = session.Url });
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

                    // Enregistrer la réservation dans la base de données
                    var reservation = new ReservationModel
                    {
                        TableId = int.Parse(session.Metadata["tableId"]),
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
                }

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur Stripe Webhook : {ex.Message}");
                return BadRequest();
            }
        }


        [HttpGet("success")]
        public IActionResult Success(string session_id)
        {
            if (string.IsNullOrEmpty(session_id))
            {
                return BadRequest("Session ID manquant.");
            }

            var service = new SessionService();
            var session = service.Get(session_id);

            // Initialiser une réservation avec validation des dates
            var reservation = new ReservationModel
            {
                TableId = int.Parse(session.Metadata["tableId"]),
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
        public string ClientEmail { get; set; }
        public string ClientPhone { get; set; }
    }
}
