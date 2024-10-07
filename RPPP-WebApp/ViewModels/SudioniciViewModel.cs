using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class SudioniciViewModel
    {
        public IEnumerable<Sudionik> Sudionici { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
