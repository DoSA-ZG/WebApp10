using FluentValidation;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.ModelsValidation

{
  
        public class AktivnostValidator : AbstractValidator<Aktivnost>
        {
        /// <summary>
        /// Sluzi za validaciju modela objekta
        /// </summary>
            public AktivnostValidator()
            {
            RuleFor(a => a.IdAktivnost)
                .NotEmpty().WithMessage("IdAktivnost je obavezno polje")
                .GreaterThan(0).WithMessage("IdAktivnost mora biti veća od 0");



            RuleFor(a => a.DatumPocetka)
                .NotEmpty().WithMessage("DatumPocetka je obavezno polje");


            RuleFor(a => a.IdVrstaAktivnosti)
                .NotEmpty().WithMessage("IdVrstaAktivnosti je obavezno polje")
                .InclusiveBetween(1, 4).WithMessage("IdVrstaAktivnosti mora biti u rasponu od 1 do 4");
            }

    }
   

}
