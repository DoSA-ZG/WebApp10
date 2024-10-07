using RPPP_WebApp.Models;
using System.Collections.Generic;

namespace RPPP_WebApp.ViewModels
{
    public class ProjektnaDokumentacijaViewModel
    {
        public IEnumerable<ProjektnaDokumentacija> ProjektnaDokumentacija { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}