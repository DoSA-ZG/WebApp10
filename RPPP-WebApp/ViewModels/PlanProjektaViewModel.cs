using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class PlanProjektaViewModel
    {

        public IEnumerable<PlanProjektum> PlanoviProjekta { get; set; }
        public PagingInfo PagingInfo { get; set; }

    }
}
