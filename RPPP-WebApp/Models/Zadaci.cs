﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models;

public partial class Zadaci
{
    public int IdZadatak { get; set; }

    public string Status { get; set; }

    public string Aktivan { get; set; }

    public string Opis { get; set; }

    public int IdZahtjev { get; set; }

    public string NositeljZadatka { get; set; }

    public int? IdPrioritetZadatka { get; set; }

    public virtual PrioritetZadatka IdPrioritetZadatkaNavigation { get; set; }

    public virtual Zahtjev IdZahtjevNavigation { get; set; }
}