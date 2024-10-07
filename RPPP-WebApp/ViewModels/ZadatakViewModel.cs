using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.ViewModels
{
    public class ZadatakViewModel
    {
        public int IdZadatak { get; set; }
        public string Status{ get; set; }
        public string Aktivan { get; set; }
        public string Opis { get; set; }
        public int IdZahtjev { get; set; }
        public string NositeljZadatka { get; set; }
        public int IdPrioritetZadatka { get; set; }
    }
}
