using FluentValidation;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.ModelsValidation
{
    public class TransakcijaValidator : AbstractValidator<Transakcija>
    {
        /// <summary>
        /// Sluzi za validaciju modela objekta
        /// </summary>
        public TransakcijaValidator()
        {
            RuleFor(t => t.IdTransakcija)
                .NotEmpty().WithMessage("IdTransakcija je obavezno polje");
            
            RuleFor(t => t.Ibanposiljatelja)
                .NotEmpty().WithMessage("IbanPosiljatelja je obavezno polje").
                Matches(@"^[A-Z]{2}\d{2}[a-zA-Z0-9]{1,30}$").WithMessage("Format IBAN-a nije ispravan"); 
            
            RuleFor(t => t.Ibanprimatelja)
               .NotEmpty().WithMessage("IbanPrimatelja je obavezno polje")
               .Matches(@"^[A-Z]{2}\d{2}[a-zA-Z0-9]{1,30}$").WithMessage("Format IBAN-a nije ispravan"); ;

            RuleFor(t => t.Iznos)
           .NotEmpty().WithMessage("Iznos je obavezno polje");

            RuleFor(t => t.IdVrstaTransakcije)
                .NotEmpty().WithMessage("IdVrstaTransakcije je obavezno polje")
                .InclusiveBetween(1, 5).WithMessage("IdVrstaTransakcije mora biti u rasponu od 1 do 5");
        }
    }
}