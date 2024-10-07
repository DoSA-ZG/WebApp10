using FluentValidation;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.ModelsValidation

{
  
        public class ProjektValidator : AbstractValidator<Projekt>
        {
        /// <summary>
        /// Sluzi za validaciju modela objekta
        /// </summary>
        public ProjektValidator()
            { 
            
            RuleFor(p => p.IdProjekt)
                .NotEmpty().WithMessage("IdProjekta je obavezno polje");
            /*
            RuleFor(p => p.IdProjekt)
                .GreaterThan(-100).WithMessage("IdProjekta je mora biti pozitivan");
            */
            RuleFor(p => p.KraticaProjekta)
                .NotEmpty().WithMessage("Kratica projketa je obavezno polje");

            RuleFor(p => p.NazivProjekta)
                .NotEmpty().WithMessage("Naziv projekta je obavezno polje");

            RuleFor(p => p.DatumPocetka)
                .NotEmpty().WithMessage("Datum pocetka je obavezno polje");

            RuleFor(p => p.IdVrstaProjekta)
                .NotEmpty().WithMessage("Id vrste projekta je obavezno polje");
        }

    }
   

}
