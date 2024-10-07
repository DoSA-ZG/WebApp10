using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class ZahtjevViewModel
    {
        public int IdZahtjev { get; set; }
        public string Opis { get; set; }
        public string Prioritet { get; set; }
        public string Vrsta { get; set; }
        public IEnumerable<ZadatakViewModel> Zadacis { get; set; }
        public int IdProjekt { get; set; }

        public ZahtjevViewModel()
        {
            this.Zadacis = new List<ZadatakViewModel>();
        }
        public int IdZadatak { get; set; }
        public string Status { get; set; }
        public string Aktivan { get; set; }
        public string NositeljZadatka { get; set; }
        public int IdPrioritetZadatka { get; set; }
    }
}