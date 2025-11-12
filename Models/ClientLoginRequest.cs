using System.ComponentModel.DataAnnotations;

namespace Table_Reservation.Models
{
    public class ClientLoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Password { get; set; } = string.Empty;
    }
}

