using RPPP_WebApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.ViewModels
{
    public class AktivnostiViewModel
    {
        public int IdAktivnost { get; set; }

        public string OpisAktivnosti { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy.}", ApplyFormatInEditMode = false)]
        [DataType(DataType.Date)]
        public DateTime DatumPocetka { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy.}", ApplyFormatInEditMode = false)]
        [DataType(DataType.Date)]
        public DateTime? DatumZavrsetka { get; set; }

        public int IdVrstaAktivnosti { get; set; }

        public int IdPlanProjekta { get; set; }
    }
}
