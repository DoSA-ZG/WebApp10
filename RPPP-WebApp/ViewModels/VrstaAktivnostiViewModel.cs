using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class VrstaAktivnostiViewModel
    {
        
            public IEnumerable<VrstaAktivnosti> vrstaAktivnostis { get; set; }
            public PagingInfo PagingInfo { get; set; }
        
    }
}
