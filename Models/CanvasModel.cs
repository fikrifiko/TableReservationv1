using System.ComponentModel.DataAnnotations;

namespace Table_Reservation.Models
{
    public class CanvasModel
    {
        [Key]
        public int Id { get; set; } // Identifiant unique

        [Required]
        public int Width { get; set; } // Largeur du canvas

        [Required]
        public int Height { get; set; } // Hauteur du canvas
    }
}
