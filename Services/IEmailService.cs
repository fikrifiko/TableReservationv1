namespace Table_Reservation.Services
{
    public interface IEmailService
    {
        Task SendReservationConfirmationEmailAsync(string to, ReservationModel reservation);
    }
}
