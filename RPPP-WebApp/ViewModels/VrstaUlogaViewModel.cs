using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class VrstaUlogaViewModel
    {
        public int IdVrstaUloga { get; set; }

        public string Naziv { get; set; }

        public virtual ICollection<Uloge> Uloges { get; set; }

    }
}
