﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models;

public partial class Uloge
{
    public int IdUloga { get; set; }

    public int IdVrstaUloga { get; set; }

    public string OpisUloga { get; set; }

    public virtual VrstaUloge IdVrstaUlogaNavigation { get; set; }

    public virtual ICollection<Sudjeluju> Sudjelujus { get; set; } = new List<Sudjeluju>();
}