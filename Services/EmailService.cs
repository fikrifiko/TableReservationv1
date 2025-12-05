using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Table_Reservation.Models;

namespace Table_Reservation.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpServer = "smtp.gmail.com"; 
        private readonly int _smtpPort = 587; 
        private readonly string _smtpUser = "tablereservation9@gmail.com"; 
        private readonly string _smtpPass = "pkjv daju jtws wqhc"; 

        public async Task  SendReservationConfirmationEmailAsync(string to, ReservationModel reservation)
        {
            using (var smtpClient = new SmtpClient(_smtpServer))
            {
                smtpClient.Port = _smtpPort;
                smtpClient.Credentials = new NetworkCredential(_smtpUser, _smtpPass);
                smtpClient.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpUser),
                    Subject = "Confirmation de votre réservation",
                    Body = GenerateReservationEmailBody(reservation),
                    IsBodyHtml = true
                };

                mailMessage.To.Add(to);

                await smtpClient.SendMailAsync(mailMessage);
            }
        }

        public async Task SendReservationCancellationEmailAsync(string to, ReservationModel reservation, bool isDutch)
        {
            using (var smtpClient = new SmtpClient(_smtpServer))
            {
                smtpClient.Port = _smtpPort;
                smtpClient.Credentials = new NetworkCredential(_smtpUser, _smtpPass);
                smtpClient.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpUser),
                    Subject = isDutch ? "Bevestiging van je annulering" : "Confirmation de votre annulation",
                    Body = GenerateCancellationEmailBody(reservation, isDutch),
                    IsBodyHtml = true
                };

                mailMessage.To.Add(to);

                await smtpClient.SendMailAsync(mailMessage);
            }
        }

        public async Task SendNewsletterEmailAsync(string to, string subject, string content, bool isDutch = false)
        {
            using (var smtpClient = new SmtpClient(_smtpServer))
            {
                smtpClient.Port = _smtpPort;
                smtpClient.Credentials = new NetworkCredential(_smtpUser, _smtpPass);
                smtpClient.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpUser, "L'Etoile Blanche"),
                    Subject = subject,
                    Body = GenerateNewsletterEmailBody(content, isDutch),
                    IsBodyHtml = true
                };

                mailMessage.To.Add(to);

                await smtpClient.SendMailAsync(mailMessage);
            }
        }

        private string GenerateNewsletterEmailBody(string content, bool isDutch)
        {
            var sb = new StringBuilder();
            sb.Append("<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #ffffff;'>");
            sb.Append("<div style='text-align: center; margin-bottom: 30px;'>");
            sb.Append("<h1 style='color: #000000; font-size: 28px; margin: 0;'>L'Etoile Blanche</h1>");
            sb.Append("</div>");
            sb.Append("<div style='color: #333333; line-height: 1.6;'>");
            sb.Append(content);
            sb.Append("</div>");
            sb.Append("<div style='margin-top: 40px; padding-top: 20px; border-top: 1px solid #eeeeee; text-align: center; color: #666666; font-size: 12px;'>");
            if (isDutch)
            {
                sb.Append("<p>L'Etoile Blanche Restaurant</p>");
                sb.Append("<p>12 Rue Exemple, 75000 Paris, France</p>");
                sb.Append("<p><a href='/Home/Terms' style='color: #666666;'>Algemene voorwaarden</a></p>");
            }
            else
            {
                sb.Append("<p>L'Etoile Blanche Restaurant</p>");
                sb.Append("<p>12 Rue Exemple, 75000 Paris, France</p>");
                sb.Append("<p><a href='/Home/Terms' style='color: #666666;'>Conditions générales</a></p>");
            }
            sb.Append("</div>");
            sb.Append("</div>");

            return sb.ToString();
        }

        private string GenerateReservationEmailBody(ReservationModel reservation)
        {
            var sb = new StringBuilder();
            sb.Append("<h2>Confirmation de votre réservation</h2>");
            sb.Append($"<p>Bonjour {reservation.ClientName},</p>");
            sb.Append("<p>Merci pour votre réservation. Voici les détails :</p>");
            sb.Append("<ul>");
            sb.Append($"<li><strong>Numéro de réservation :</strong> {reservation.Id}</li>");
            sb.Append($"<li><strong>Date :</strong> {reservation.ReservationDate.ToString("dd/MM/yyyy HH:mm")}</li>");
            sb.Append($"<li><strong>Table réservée :</strong> {reservation.TableName}</li>");
            sb.Append($"<li><strong>Montant payé :</strong> {reservation.Amount} €</li>");
            sb.Append("</ul>");
            sb.Append("<p>Nous avons hâte de vous accueillir !</p>");
            sb.Append("<p>Cordialement,</p>");
            sb.Append("<p><strong>Votre Restaurant</strong></p>");

            return sb.ToString();
        }

        private string GenerateCancellationEmailBody(ReservationModel reservation, bool isDutch)
        {
            var sb = new StringBuilder();
            if (isDutch)
            {
                sb.Append("<h2>Bevestiging van je annulering</h2>");
                sb.Append($"<p>Beste {reservation.ClientName},</p>");
                sb.Append("<p>We bevestigen de annulering van je reservering. Hieronder vind je een samenvatting:</p>");
                sb.Append("<ul>");
                sb.Append($"<li><strong>Reservatie-ID:</strong> {reservation.Id}</li>");
                sb.Append($"<li><strong>Datum en uur:</strong> {reservation.ReservationHoure:dd/MM/yyyy HH:mm}</li>");
                sb.Append($"<li><strong>Tafel:</strong> {reservation.TableName}</li>");
                sb.Append("</ul>");
                sb.Append("<p>We hopen je binnenkort opnieuw te mogen verwelkomen.</p>");
                sb.Append("<p>Met vriendelijke groeten,<br><strong>Je restaurant</strong></p>");
            }
            else
            {
                sb.Append("<h2>Confirmation de votre annulation</h2>");
                sb.Append($"<p>Bonjour {reservation.ClientName},</p>");
                sb.Append("<p>Nous confirmons l'annulation de votre réservation. Voici un rappel des informations :</p>");
                sb.Append("<ul>");
                sb.Append($"<li><strong>Numéro de réservation :</strong> {reservation.Id}</li>");
                sb.Append($"<li><strong>Date et heure :</strong> {reservation.ReservationHoure:dd/MM/yyyy HH:mm}</li>");
                sb.Append($"<li><strong>Table :</strong> {reservation.TableName}</li>");
                sb.Append("</ul>");
                sb.Append("<p>Nous espérons avoir le plaisir de vous accueillir une prochaine fois.</p>");
                sb.Append("<p>Cordialement,<br><strong>Votre restaurant</strong></p>");
            }

            return sb.ToString();
        }
    }
}
