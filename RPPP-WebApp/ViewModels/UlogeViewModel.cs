using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class UlogeViewModel
    {
        public IEnumerable<Uloge> Uloge { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}