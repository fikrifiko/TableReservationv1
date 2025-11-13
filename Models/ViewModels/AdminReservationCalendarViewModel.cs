using System;
using System.Collections.Generic;

namespace Table_Reservation.Models.ViewModels
{
    public class AdminReservationCalendarViewModel
    {
        public IList<IList<AdminReservationCalendarDayViewModel>> Weeks { get; set; } = new List<IList<AdminReservationCalendarDayViewModel>>();
        public DateTime MonthReference { get; set; }
        public int MonthOffset { get; set; }
        public bool IsDutch { get; set; }
    }

    public class AdminReservationCalendarDayViewModel
    {
        public DateTime Date { get; set; }
        public bool IsInCurrentMonth { get; set; }
        public bool IsToday { get; set; }
        public IList<AdminReservationSummaryViewModel> Reservations { get; set; } = new List<AdminReservationSummaryViewModel>();
    }

    public class AdminReservationSummaryViewModel
    {
        public int Id { get; set; }
        public string TableName { get; set; } = string.Empty;
        public DateTime Start { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ClientEmail { get; set; } = string.Empty;
        public string ClientPhone { get; set; } = string.Empty;
        public int Amount { get; set; }
        public bool IsCancelled { get; set; }
        public DateTime? CancelledAt { get; set; }
        public bool ClientProfileDeleted { get; set; }
    }
}




