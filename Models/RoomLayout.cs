namespace Table_Reservation.Models
{
    public class RoomLayout
    {
        public int Id { get; set; }
        public string Name { get; set; } // Ex: "Main Hall"
        public List<TableModel> Tables { get; set; } = new List<TableModel>();
    }




}
