using System.ComponentModel.DataAnnotations.Schema;
using Table_Reservation.Models;

public class ReservationModel
{
    public int Id { get; set; }
    public int TableId { get; set; }
    public string TableName { get; set; } = string.Empty;
    public DateTime ReservationDate { get; set; }
    public DateTime ReservationHoure { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public string ClientPhone { get; set; } = string.Empty;
    public int Amount { get; set; }

    public bool IsCancelled { get; set; }
    public DateTime? CancelledAt { get; set; }

    [ForeignKey(nameof(Client))]
    public int? ClientId { get; set; }
    public ClientModel? Client { get; set; }
}
