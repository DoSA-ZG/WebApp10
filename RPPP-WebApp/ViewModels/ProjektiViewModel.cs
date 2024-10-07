using RPPP_WebApp.Models;
using System.Collections.Generic;

namespace RPPP_WebApp.ViewModels
{
    public class ProjektiViewModel
    {
        public IEnumerable<Projekt> projekti { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}