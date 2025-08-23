namespace Table_Reservation.Models
{
    public class AdministratorModel
    {

        public int Id { get; set; } // Clé primaire
        public string Username { get; set; } // Nom d'utilisateur
        public string Password { get; set; } // Mot de passe (hashé)
        public string Email { get; set; } // Adresse e-mail
        public DateTime CreatedAt { get; set; } // Date de création

    }
}
