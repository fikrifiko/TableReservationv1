using System.Collections.Generic;

namespace Table_Reservation.Models.ViewModels
{
    public class ClientReservationListItem
    {
        public int Id { get; set; }
        public string TableName { get; set; } = string.Empty;
        public DateTime ReservationDate { get; set; }
        public DateTime ReservationStart { get; set; }
        public int Amount { get; set; }
        public bool IsCancelled { get; set; }
        public bool CanCancel { get; set; }
    }

    public class ClientDashboardViewModel
    {
        public string ClientName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public IList<ClientReservationListItem> Reservations { get; set; } = new List<ClientReservationListItem>();
    }
}

