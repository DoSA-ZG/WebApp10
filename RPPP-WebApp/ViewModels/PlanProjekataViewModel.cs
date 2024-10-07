using RPPP_WebApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.ViewModels
{
    public class PlanProjekataViewModel
    {
        public int IdPlanProjekta { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy.}", ApplyFormatInEditMode = false)]
        [DataType(DataType.Date)]
        public DateTime PlaniraniPocetakZadatka { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy.}", ApplyFormatInEditMode = false)]
        [DataType(DataType.Date)]
        public DateTime StvarniPocetakZadatka { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy.}", ApplyFormatInEditMode = false)]
        [DataType(DataType.Date)]
        public DateTime PlaniraniZavrsetakZadatka { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy.}", ApplyFormatInEditMode = false)]
        [DataType(DataType.Date)]
        public DateTime? StvarniZavrsetakZadatka { get; set; }

        public int IdProjekt { get; set; }

        public int IdVoditelj { get; set; }

        public int IdAktivnost { get; set; }

        public IEnumerable<AktivnostiViewModel> IdAktivnosts { get; set; }

        public PlanProjekataViewModel()
        {
            this.IdAktivnosts = new List<AktivnostiViewModel>();
        }

        public string OpisAktivnosti { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy.}", ApplyFormatInEditMode = false)]
        [DataType(DataType.Date)]
        public DateTime DatumPocetka { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy.}", ApplyFormatInEditMode = false)]
        [DataType(DataType.Date)]
        public DateTime? DatumZavrsetka { get; set; }

        public int IdVrstaAktivnosti { get; set; }
    }
}
