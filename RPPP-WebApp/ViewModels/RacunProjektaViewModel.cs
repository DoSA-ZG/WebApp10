using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class RacunProjektaViewModel
    {
        public string Iban { get; set; }

        public string NazivProjekta { get; set; }

        public string StanjeRacuna { get; set; }

        public int IdProjekt { get; set; }
        public virtual ICollection<TransakcijaViewModel> IdTransakcijas { get; set; }

        public int IdTransakcija { get; set; }

        public string Ibanposiljatelja { get; set; }

        public string Ibanprimatelja { get; set; }

        public string OpisTransakcije { get; set; }

        public string Iznos { get; set; }

        public int IdVrstaTransakcije { get; set; }

    }
}
