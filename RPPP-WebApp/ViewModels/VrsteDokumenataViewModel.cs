using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class VrsteDokumenataViewModel
    {

        public IEnumerable<VrstaDokumentum> vrsteDokumenata { get; set; }
        public PagingInfo PagingInfo { get; set; }

    }
}
