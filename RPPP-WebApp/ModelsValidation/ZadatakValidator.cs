using FluentValidation;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.ModelsValidation

{
  
        public class ZadatakValidator : AbstractValidator<Zadaci>
        {
        /// <summary>
        /// Sluzi za validaciju modela objekta
        /// </summary>
        public ZadatakValidator()
            {
            RuleFor(z => z.IdZadatak)
                .NotEmpty().WithMessage("IdZadatak je obavezno polje");
                 

                RuleFor(z => z.Status)
                    .NotEmpty().WithMessage("Status je obavezno polje");


            RuleFor(z => z.Aktivan)
                    .NotEmpty().WithMessage("Aktivan je obavezno polje")
                    .Length(2).WithMessage("Aktivan mora biti duljine 2");


            RuleFor(z => z.IdZahtjev)
                .NotEmpty().WithMessage("IdZahtjev je obavezno polje");
                   

                RuleFor(z => z.NositeljZadatka)
                    .NotEmpty().WithMessage("NositeljZadatka je obavezno polje");

                RuleFor(z => z.IdPrioritetZadatka)
                    .NotEmpty().WithMessage("IdPrioritetZadatka je obavezno polje")
                    .InclusiveBetween(1, 3).WithMessage("IdPrioritetZadatka mora biti u rasponu od 1 do 4");
            }

    }
   

}
