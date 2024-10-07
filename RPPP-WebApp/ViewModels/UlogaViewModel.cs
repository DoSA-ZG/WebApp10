using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class UlogaViewModel
    {
        public int IdUloga { get; set; }

        public int IdVrstaUloga { get; set; }

        public string OpisUloga { get; set; }

        public virtual ICollection<Sudjeluju> Sudjelujus { get; set; }
    }
}
