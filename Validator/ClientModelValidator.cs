using FluentValidation;
using Table_Reservation.Models;

namespace Table_Reservation.Validator
{
    public class ClientModelValidator: AbstractValidator<ClientModel>
    {
        public ClientModelValidator()
        {
            //Regle pour dire qu'il faut que le nom client ne soit pas empty
            RuleFor(client => client.ClientName)
                .NotEmpty().WithMessage("Client Name Is Required");

            //Regle pour dire qu'il faut que le email client est required et que le format doit etre une adresse mail. càd avec le @ et le .be ou .com etc.
            RuleFor(client => client.ClientEmail)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            //Regle pour dire que le numéro de telepgone doit au moins contenir 9 charactere (possivilté d'ajouter d'autres regles, mais pour l'instant on fait comme ca.
            RuleFor(client => client.ClientPhone).NotEmpty()
                .MinimumLength(9).WithMessage("Please enter a valid phone number");
        }
    }
}
