using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class VrsteProjekataViewModel
    {

        public IEnumerable<VrstaProjektum> vrsteProjekta { get; set; }
        public PagingInfo PagingInfo { get; set; }

    }
}
