using FluentValidation;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.ModelsValidation
{
    public class UlogaValidator : AbstractValidator<Uloge>
    {
        /// <summary>
        /// Sluzi za validaciju modela objekta
        /// </summary>
        public UlogaValidator()
        {
            RuleFor(u => u.IdUloga)
                .NotEmpty().WithMessage("IdUloge je obavezno polje");

            RuleFor(u => u.IdVrstaUloga)
                .NotEmpty().WithMessage("IdUloge je obavezno polje");

            RuleFor(u => u.OpisUloga)
                .NotEmpty().WithMessage("IdUloge je obavezno polje");
        }
    }
}
