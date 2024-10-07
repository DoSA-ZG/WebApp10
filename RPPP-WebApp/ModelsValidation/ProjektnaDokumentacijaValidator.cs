using FluentValidation;
using RPPP_WebApp.Controllers;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.ModelsValidation

{

    public class projektnaDokumentacijaValidator : AbstractValidator<ProjektnaDokumentacija>
    {
        /// <summary>
        /// Sluzi za validaciju modela objekta
        /// </summary>
        public projektnaDokumentacijaValidator()
        {
            RuleFor(pd => pd.IdDokument)
                .NotEmpty().WithMessage("IdDokumenta je obavezno polje");
           /*
            RuleFor(pd => pd.IdDokument)
                .GreaterThan(0).WithMessage("IdDokumenta je obavezno pozitivan");
           */
            RuleFor(pd => pd.NazivDokumenta)
                .NotEmpty().WithMessage("Naziv dokumenta je obavezno polje");

            RuleFor(pd => pd.IdVrstaDokument)
                .NotEmpty().WithMessage("Id vrsta dokumenta je obavezno polje");


            RuleFor(pd => pd.FormatDokumenta)
                .NotEmpty().WithMessage("Format dokumenta je obavezno polje");


            RuleFor(pd => pd.IdProjekt)
                .NotEmpty().WithMessage("Id projekta je obavezno polje");

        }

    }


}
