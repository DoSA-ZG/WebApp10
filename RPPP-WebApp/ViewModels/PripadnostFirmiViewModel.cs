using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class PripadnostFirmiViewModel
    {
        public int IdFirma { get; set; }

        public string NazivFirma { get; set; }

        public string AdresaSjedista { get; set; }

        public virtual ICollection<Sudionik> Sudioniks { get; set; }
    }
}
