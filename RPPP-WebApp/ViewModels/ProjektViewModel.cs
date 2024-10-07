using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class ProjektViewModel
    {
        public int IdProjekt{ get; set; }
        public string KraticaProjekta { get; set; }
        public string NazivProjekta { get; set; }
        public DateTime DatumPocetka { get; set; }
        public DateTime? DatumZavrsetka { get; set; }
        public int IdVrstaProjekta { get; set; }
        public virtual ICollection<ProjektnaDokumentacija> ProjektnaDokumentacijas { get; set; }
        public int IdDokument { get; set; }
        public string NazivDokumenta { get; set; }
        public int IdVrstaDokument { get; set; }
        public string FormatDokumenta { get; set; }

    }
}
