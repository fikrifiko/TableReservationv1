using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Table_Reservation.Models
{
    public class ClientModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Indique que l'ID est généré automatiquement
        public int Id { get; set; }

        [Required] // Indique que ce champ est obligatoire
        [MaxLength(100)] // Définit la longueur maximale du nom
        public string ClientName { get; set; }

        [Required] // Indique que ce champ est obligatoire
        [EmailAddress] // Valide que l'entrée est une adresse email valide
        [MaxLength(200)] // Définit la longueur maximale pour l'email
        public string ClientEmail { get; set; }

        [Required] // Indique que ce champ est obligatoire
        [MaxLength(15)] // Définit la longueur maximale pour le téléphone
        public string ClientPhone { get; set; }
    }
}
