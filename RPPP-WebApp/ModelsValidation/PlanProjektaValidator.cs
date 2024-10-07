using FluentValidation;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.ModelsValidation
{
    public class PlanProjektaValidator : AbstractValidator<PlanProjektum>
    {
        /// <summary>
        /// Sluzi za validaciju modela objekta
        /// </summary>
        public PlanProjektaValidator()
        {
            RuleFor(p => p.IdPlanProjekta)
                .NotEmpty().WithMessage("IdPlanProjekta je obavezno polje")
                .GreaterThan(0).WithMessage("IdPlanProjekta mora biti veći od 0"); ;

            RuleFor(p => p.PlaniraniPocetakZadatka)
                .NotEmpty().WithMessage("PlaniraniPocetakZadatka je obavezno polje");

            RuleFor(p => p.PlaniraniZavrsetakZadatka)
                .NotEmpty().WithMessage("PlaniraniZavrsetakZadatka je obavezno polje");

            RuleFor(p => p.StvarniPocetakZadatka)
                .NotEmpty().WithMessage("StvarniPocetakZadatka je obavezno polje");

            RuleFor(p => p.IdProjekt)
                .NotEmpty().WithMessage("IdProjekt je obavezno polje");

            RuleFor(p => p.IdVoditelj)
                .NotEmpty().WithMessage("IdVoditelj je obavezno polje")
                .InclusiveBetween(1, 5).WithMessage("IdVoditelj mora biti u rasponu od 1 do 5");
        }
    }
}
