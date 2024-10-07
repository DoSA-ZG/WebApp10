using RPPP_WebApp.Models;
using System.Collections.Generic;

namespace RPPP_WebApp.ViewModels
{
    public class ZadaciViewModel
    {
        public IEnumerable<Zadaci> Zadaci { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}