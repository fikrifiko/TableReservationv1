using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using Table_Reservation.Models;
using BCryptNet = BCrypt.Net.BCrypt;

namespace Table_Reservation.Services
{
    public class ClientAccountService
    {
        private readonly AppDbContext _context;

        public ClientAccountService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ClientModel> GetOrCreateClientAsync(string name, string email, string phone)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            var normalizedPhone = phone.Trim();
            var trimmedName = name.Trim();

            var client = await _context.ClientModels
                .SingleOrDefaultAsync(c => c.ClientEmail == normalizedEmail);

            var passwordHash = BCryptNet.HashPassword(normalizedPhone);
            if (client == null)
            {
                client = new ClientModel
                {
                    ClientName = trimmedName,
                    ClientEmail = normalizedEmail,
                    ClientPhone = normalizedPhone,
                    PasswordHash = passwordHash,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ClientModels.Add(client);
                await _context.SaveChangesAsync();
                return client;
            }

            bool requiresUpdate = false;

            if (!string.Equals(client.ClientName, trimmedName, StringComparison.Ordinal))
            {
                client.ClientName = trimmedName;
                requiresUpdate = true;
            }

            if (!string.Equals(client.ClientEmail, normalizedEmail, StringComparison.Ordinal))
            {
                client.ClientEmail = normalizedEmail;
                requiresUpdate = true;
            }

            if (!string.Equals(client.ClientPhone, normalizedPhone, StringComparison.Ordinal))
            {
                client.ClientPhone = normalizedPhone;
                requiresUpdate = true;
            }

            var passwordMatches = false;
            var passwordNeedsUpgrade = false;
            try
            {
                passwordMatches = BCryptNet.Verify(normalizedPhone, client.PasswordHash);
            }
            catch
            {
                if (string.Equals(client.PasswordHash, normalizedPhone, StringComparison.Ordinal))
                {
                    passwordMatches = true;
                    passwordNeedsUpgrade = true;
                }
            }

            if (!passwordMatches || passwordNeedsUpgrade)
            {
                client.PasswordHash = passwordHash;
                requiresUpdate = true;
            }

            if (requiresUpdate)
            {
                client.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return client;
        }

        public async Task<bool> ReservationExistsAsync(int tableId, string clientEmail, DateTime reservationDate, DateTime reservationStart)
        {
            var normalizedEmail = clientEmail.Trim().ToLowerInvariant();
            return await _context.Reservations.AnyAsync(r =>
                !r.IsCancelled &&
                r.TableId == tableId &&
                r.ClientEmail == normalizedEmail &&
                r.ReservationDate == reservationDate &&
                r.ReservationHoure == reservationStart);
        }

        public async Task<ReservationModel> CreateReservationAsync(
            int tableId,
            string tableName,
            string clientName,
            string clientEmail,
            string clientPhone,
            DateTime reservationDate,
            DateTime reservationStart,
            long amountInCents)
        {
            var client = await GetOrCreateClientAsync(clientName, clientEmail, clientPhone);

            return new ReservationModel
            {
                TableId = tableId,
                TableName = tableName,
                ClientName = client.ClientName,
                ClientEmail = client.ClientEmail,
                ClientPhone = client.ClientPhone,
                ReservationDate = reservationDate,
                ReservationHoure = reservationStart,
                Amount = (int)(amountInCents / 100),
                ClientId = client.Id,
                Client = client,
                IsCancelled = false
            };
        }

        public static DateTime CombineDateAndTime(DateTime date, string timeString)
        {
            if (string.IsNullOrWhiteSpace(timeString))
            {
                return date;
            }

            if (!TimeSpan.TryParse(timeString, CultureInfo.InvariantCulture, out var timePart))
            {
                timePart = TimeSpan.Zero;
            }

            return date.Add(timePart);
        }
    }
}

