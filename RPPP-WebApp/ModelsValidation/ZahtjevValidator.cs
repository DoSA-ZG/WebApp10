using FluentValidation;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.ModelsValidation
{
    public class ZahtjevValidator : AbstractValidator<Zahtjev>
    {
        /// <summary>
        /// Sluzi za validaciju modela objekta
        /// </summary>
        public ZahtjevValidator()
        {
            RuleFor(z => z.IdZahtjev)
                .NotEmpty().WithMessage("IdZahtjev je obavezno polje");

            RuleFor(z => z.Opis)
                .NotEmpty().WithMessage("Opis je obavezno polje");

            RuleFor(z => z.Prioritet)
                .NotEmpty().WithMessage("Prioritet je obavezno polje");

            RuleFor(z => z.Vrsta)
                .NotEmpty().WithMessage("Vrsta je obavezno polje");
        }
    }
}
