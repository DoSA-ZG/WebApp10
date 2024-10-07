using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class PripadnostiFirmiViewModel
    {
        public IEnumerable<PripadnostFirmi> PripadnostFirmis { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
