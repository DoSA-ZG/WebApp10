﻿using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels
{
    public class SudionikViewModel
    {
        
        public int IdSudionik { get; set; }

        public string Email { get; set; }

        public string Kontakt { get; set; }

        public string AdresaUreda { get; set; }

        public int IdFirma { get; set; }

        public int IdProjekt { get; set; }

        public int IdUloga { get; set; }

        public int IdVrstaUloga { get; set; }

        public string OpisUloga { get; set; }

        public virtual ICollection<Sudjeluju> Sudjelujus { get; set; }
    }
}

