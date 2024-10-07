using FluentValidation;
using RPPP_WebApp.Models;
 
namespace RPPP_WebApp.ModelsValidation
{

    public class RacunProjektaValidator : AbstractValidator<RacunProjektum>

    {
        /// <summary>
        /// Sluzi za validaciju modela objekta
        /// </summary>
        public RacunProjektaValidator()

        {

            RuleFor(rp => rp.Iban)

                .NotEmpty().WithMessage("IBAN je obavezno polje").
                           Matches(@"^[A-Z]{2}\d{2}[a-zA-Z0-9]{1,30}$")
                           .WithMessage("Format IBAN-a nije ispravan");

            RuleFor(rp => rp.NazivProjekta)

                .NotEmpty().WithMessage("Naziv projekta je obavezno polje");

            RuleFor(rp => rp.StanjeRacuna)

                .NotEmpty().WithMessage("Stanje računa je obavezno polje");

            
        }

    }

}
