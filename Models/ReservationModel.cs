using System.ComponentModel.DataAnnotations;
using Table_Reservation.Models;


public class ReservationModel
{
    public int Id { get; set; }
    public int TableId { get; set; }
    public string TableName { get; set; }
    public DateTime ReservationDate { get; set; }
    public DateTime ReservationHoure { get; set; }
    public string ClientName { get; set; }
    public string ClientEmail { get; set; }
    public string ClientPhone { get; set; }
    public int Amount { get; set; }

}
