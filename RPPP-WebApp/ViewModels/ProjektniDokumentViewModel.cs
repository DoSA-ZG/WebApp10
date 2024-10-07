using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.ViewModels
{
    public class ProjektniDokumentViewModel
    {
        public int IdDokument{ get; set; }
        public string NazivDokument { get; set; }
        public int IdVrstaDokument { get; set; }
        public string Format { get; set; }
        public int IdProjekt { get; set; }

    }
}
