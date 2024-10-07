using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class RacuniProjektaViewModel
    {
        public IEnumerable<RacunProjektum> Racuni { get; set; }

        public PagingInfo PagingInfo { get; set; }
    }
}
