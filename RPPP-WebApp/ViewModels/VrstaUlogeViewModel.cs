using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class VrstaUlogeViewModel
    {
        public IEnumerable<VrstaUloge> vrstaUloges { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
