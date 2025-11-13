using System;
using System.Collections.Generic;

namespace Table_Reservation.Models.ViewModels
{
    public class AdminClientsViewModel
    {
        public IList<AdminClientListItem> Clients { get; set; } = new List<AdminClientListItem>();
        public string Query { get; set; } = string.Empty;
        public bool IsDutch { get; set; }
    }

    public class AdminClientListItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int ReservationsCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}




