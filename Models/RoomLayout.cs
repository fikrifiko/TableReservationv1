namespace Table_Reservation.Models
{
    public class RoomLayout
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CanvasWidth { get; set; }
        public int CanvasHeight { get; set; }

        public ICollection<TableModel> Tables { get; set; } = new List<TableModel>();
    }




}