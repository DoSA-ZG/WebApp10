using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class VoditeljProjektaViewModel
    {

        public IEnumerable<VoditeljProjektum> VoditeljProjekta { get; set; }
        public PagingInfo PagingInfo { get; set; }

    }
}
