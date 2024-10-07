using FluentValidation;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.ModelsValidation
{
    public class SudionikValidator : AbstractValidator<Sudionik>
    {
        /// <summary>
        /// Sluzi za validaciju modela objekta
        /// </summary>
        public SudionikValidator()
        {
            RuleFor(s => s.IdSudionik)
                .NotEmpty().WithMessage("IdSudionik je obavezno polje");

            RuleFor(s => s.Email)
                .NotEmpty().WithMessage("Email je obavezno polje");

            RuleFor(s => s.Kontakt)
                .NotEmpty().WithMessage("Kontakt je obavezno polje");

            RuleFor(s => s.IdFirma)
                .NotEmpty().WithMessage("IdFirma je obavezno polje");
        }
    }
}