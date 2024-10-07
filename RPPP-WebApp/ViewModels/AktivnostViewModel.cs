using RPPP_WebApp.Models;
using System.Collections.Generic;

namespace RPPP_WebApp.ViewModels
{
    public class AktivnostViewModel
    {
        public IEnumerable<Aktivnost> Aktivnost { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}