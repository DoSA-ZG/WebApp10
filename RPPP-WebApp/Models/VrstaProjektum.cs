﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models;

public partial class VrstaProjektum
{
    public int IdVrstaProjekta { get; set; }

    public string NazivVrsteProjekta { get; set; }

    public virtual ICollection<Projekt> Projekts { get; set; } = new List<Projekt>();
}