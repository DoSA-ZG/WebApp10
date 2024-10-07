using RPPP_WebApp.Models;
using System.Collections.Generic;

namespace RPPP_WebApp.ViewModels
{
    public class ZahtjeviViewModel
    {
        public IEnumerable<Zahtjev> Zahtjev { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}