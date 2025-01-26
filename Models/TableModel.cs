using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Table_Reservation.Models
{
    public class TableModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Indique que l'ID est généré automatiquement
        public int Id { get; set; }

        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Seats { get; set; }
        public bool Rotated { get; set; }
        public bool Reserved {  get; set; }
        public string Name { get; set; }
        }
}
