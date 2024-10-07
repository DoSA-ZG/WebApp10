using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{

    public class VrstaTransakcijaViewModel
    {
        public IEnumerable<VrstaTransakcije> VrstaTransakcijes { get; set; }

        public PagingInfo PagingInfo { get; set; }


    }

}