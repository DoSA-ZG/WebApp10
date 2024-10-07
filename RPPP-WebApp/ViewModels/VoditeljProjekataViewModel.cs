using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.ViewModels
{
    public class VoditeljProjekataViewModel
    {
        public int IdVoditelj { get; set; }

        public string ImeVoditelja { get; set; }

        public string PrezimeVoditelja { get; set; }

        public int? Oib { get; set; }
    }
}
