using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class PrioritetZadatakaViewModel
    {
        
            public IEnumerable<PrioritetZadatka> prioritetZadatakas { get; set; }
            public PagingInfo PagingInfo { get; set; }
        
    }
}
