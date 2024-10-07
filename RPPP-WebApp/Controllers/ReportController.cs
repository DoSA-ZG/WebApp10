using RPPP_WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using OfficeOpenXml;
using RPPP_WebApp.Extensions;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Reflection.Metadata;

namespace RPPP_WebApp.Controllers
{
    public class ReportController : Controller
    {
        private readonly ProjektContext ctx;
        private readonly IWebHostEnvironment environment;
        private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public ReportController(ProjektContext ctx, IWebHostEnvironment environment)
        {
            this.ctx = ctx;
            this.environment = environment;
        }
        /// <summary>
        /// Stvara excel file popunjen podacima
        /// </summary>
        /// <returns>Vraca excel file</returns>
        public async Task<IActionResult> ZadatakExcel()
        {
            var zadatak = await ctx.Zadacis
                                  .AsNoTracking()
                                  .Include(d => d.IdPrioritetZadatkaNavigation)
                                  .Include(d => d.IdZahtjevNavigation)
                                  .OrderBy(d => d.IdZadatak)
                                  .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis zadataka";
                excel.Workbook.Properties.Author = "FER-ZPR";
                var worksheet = excel.Workbook.Worksheets.Add("Zadaci");

                //First add the headers
                worksheet.Cells[1, 1].Value = "IdZadatak";
                worksheet.Cells[1, 2].Value = "Status";
                worksheet.Cells[1, 3].Value = "Aktivan";
                worksheet.Cells[1, 4].Value = "Opis";
                worksheet.Cells[1, 5].Value = "IdZahtjev";
                worksheet.Cells[1, 6].Value = "NositeljZadatka";
                worksheet.Cells[1, 7].Value = "IdPrioritetZadatka";

                for (int i = 0; i < zadatak.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = zadatak[i].IdZadatak;
                    worksheet.Cells[i + 2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[i + 2, 2].Value = zadatak[i].Status;
                    worksheet.Cells[i + 2, 3].Value = zadatak[i].Aktivan;
                    worksheet.Cells[i + 2, 4].Value = zadatak[i].Opis;
                    worksheet.Cells[i + 2, 5].Value = zadatak[i].IdZahtjevNavigation.Opis;
                    worksheet.Cells[i + 2, 6].Value = zadatak[i].NositeljZadatka;
                    worksheet.Cells[i + 2, 7].Value = zadatak[i].IdPrioritetZadatkaNavigation.RazinaPrioriteta;
                }

                worksheet.Cells[1, 1, zadatak.Count + 1, 4].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "zadatak.xlsx");
        }
        /// <summary>
        /// Stvara excel file popunjen podacima
        /// </summary>
        /// <returns>Vraca excel file</returns>
        public async Task<IActionResult> ZahtjevExcel()
        {
            var zahtjev = await ctx.Zahtjevs
                                  .AsNoTracking()
                                  .Include(d => d.IdProjekts)
                                  .OrderBy(d => d.IdZahtjev)
                                  .ToListAsync();
            var zadatak = await ctx.Zadacis
                              .AsNoTracking()
                              .OrderBy(d => d.IdZadatak)
                              .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis zahtjeva";
                excel.Workbook.Properties.Author = "FER-ZPR";
                var worksheet = excel.Workbook.Worksheets.Add("Zahtjevi");

                //First add the headers
                worksheet.Cells[1, 1].Value = "IdZahtjev";
                worksheet.Cells[1, 2].Value = "Opis";
                worksheet.Cells[1, 3].Value = "Prioritet";
                worksheet.Cells[1, 4].Value = "Vrsta";
                worksheet.Cells[1, 5].Value = "NositeljZadatka";
                worksheet.Cells[1, 6].Value = "Projekti";

                for (int i = 0; i < zahtjev.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = zahtjev[i].IdZahtjev;
                    worksheet.Cells[i + 2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[i + 2, 2].Value = zahtjev[i].Opis;
                    worksheet.Cells[i + 2, 3].Value = zahtjev[i].Prioritet;
                    worksheet.Cells[i + 2, 4].Value = zahtjev[i].Vrsta;
                    // Inicijalizacija varijable za pohranu spojenih imena nositelja
                    string spojenaImena = "";

                    // Iteracija kroz zadatak listu
                    for (int k = 0; k < zadatak.Count; k++)
                    {
                        // Provjera uvjeta
                        if (zadatak[k].IdZahtjev == zahtjev[i].IdZahtjev)
                        {
                            // Dodaj nositelja zadatka u spojenu varijablu
                            spojenaImena += zadatak[k].NositeljZadatka + ", ";
                        }
                    }

                    // Ukloni zarez i razmak s kraja spojene varijable
                    spojenaImena = spojenaImena.TrimEnd(',', ' ');

                    // Postavljanje vrijednosti u ćeliju
                    worksheet.Cells[i + 2, 5].Value = spojenaImena;
                    string spojeniProjekti = "";

                    // Dodajte novu petlju za prolazak kroz kolekciju projekata
                    foreach (var projekt in zahtjev[i].IdProjekts)
                    {
                        spojeniProjekti += projekt.NazivProjekta + ", ";
                    }

                    // Ukloni zarez i razmak s kraja spojene varijable
                    spojeniProjekti = spojeniProjekti.TrimEnd(',', ' ');

                    // Postavljanje vrijednosti u ćeliju za projekte
                    worksheet.Cells[i + 2, 6].Value = spojeniProjekti;
                }
                worksheet.Cells[1, 1, zahtjev.Count + 1, 4].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "zahtjev.xlsx");
        }
        /// <summary>
        /// Stvara excel file popunjen master-detail podacima
        /// </summary>
        /// <param name="id">Id master podatka</param>
        /// <returns>Vraca excel file</returns>
        public async Task<IActionResult> MDZahtjevExcel(int id)
        {
            var zahtjev = await ctx.Zahtjevs
                                  .AsNoTracking()
                                  .Include(d => d.IdProjekts)
                                  .OrderBy(d => d.IdZahtjev)
                                  .ToListAsync();
            var zadatak = await ctx.Zadacis
                              .AsNoTracking()
                              .Include(d => d.IdPrioritetZadatkaNavigation)
                              .OrderBy(d => d.IdZadatak)
                              .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis zahtjeva";
                excel.Workbook.Properties.Author = "FER-ZPR";
                var worksheet = excel.Workbook.Worksheets.Add("Zahtjevi");

                //First add the headers
                worksheet.Cells[1, 1].Value = "IdZahtjev";
                worksheet.Cells[1, 2].Value = "Opis";
                worksheet.Cells[1, 3].Value = "Prioritet";
                worksheet.Cells[1, 4].Value = "Vrsta";
                worksheet.Cells[1, 5].Value = "Naziv projekta";
                worksheet.Cells[1, 6].Value = "IdZadatak";
                worksheet.Cells[1, 7].Value = "Status";
                worksheet.Cells[1, 8].Value = "Aktivan";
                worksheet.Cells[1, 9].Value = "Opis Zadatka";
                worksheet.Cells[1, 10].Value = "NositeljZadatka";
                worksheet.Cells[1, 11].Value = "Prioritet zadatka";

                for (int i = 0; i < zahtjev.Count; i++)
                {
                    if (zahtjev[i].IdZahtjev == id){ 
                    worksheet.Cells[0 + 2, 1].Value = zahtjev[i].IdZahtjev;
                    worksheet.Cells[0 + 2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[0 + 2, 2].Value = zahtjev[i].Opis;
                    worksheet.Cells[0 + 2, 3].Value = zahtjev[i].Prioritet;
                    worksheet.Cells[0 + 2, 4].Value = zahtjev[i].Vrsta;
                        string spojeniProjekti = "";

                        // Dodajte novu petlju za prolazak kroz kolekciju projekata
                        foreach (var projekt in zahtjev[i].IdProjekts)
                        {
                            spojeniProjekti += projekt.NazivProjekta + ", ";
                        }

                        // Ukloni zarez i razmak s kraja spojene varijable
                        spojeniProjekti = spojeniProjekti.TrimEnd(',', ' ');

                        // Postavljanje vrijednosti u ćeliju za projekte
                        worksheet.Cells[0 + 2, 5].Value = spojeniProjekti;
                        int rowIndex = 3; // Početni redak za unos podataka o zadacima

                        // Iteracija kroz zadatak listu
                        for (int k = 0; k < zadatak.Count; k++)
                        {
                            // Provjera uvjeta
                            if (zadatak[k].IdZahtjev == zahtjev[i].IdZahtjev)
                            {
                                // Postavljanje vrijednosti za svaki zadatak u novi redak prema dolje
                                worksheet.InsertRow(rowIndex, 1);
                                worksheet.Cells[rowIndex - 1, 6].Value = zadatak[k].IdZadatak;
                                worksheet.Cells[rowIndex - 1, 7].Value = zadatak[k].Status;
                                worksheet.Cells[rowIndex - 1, 8].Value = zadatak[k].Aktivan;
                                worksheet.Cells[rowIndex - 1, 9].Value = zadatak[k].Opis;
                                worksheet.Cells[rowIndex - 1, 10].Value = zadatak[k].NositeljZadatka;
                                worksheet.Cells[rowIndex - 1, 11].Value = zadatak[k].IdPrioritetZadatkaNavigation.RazinaPrioriteta;

                                rowIndex++; // Povećajte indeks retka za sljedeći zadatak
                            }
                        }

                    }
                }
                worksheet.Cells[1, 1, zahtjev.Count + 1, 4].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "master-detail-zahtjev.xlsx");
        }
        /// <summary>
        /// Stvara excel file popunjen podacima
        /// </summary>
        /// <returns>Vraca excel file</returns>
        public async Task<IActionResult> TransakcijaExcel()
        {
            var transakcija = await ctx.Transakcijas
                                  .AsNoTracking()
                                  .Include(d => d.IdVrstaTransakcijeNavigation)
                                  .OrderBy(d => d.IdTransakcija)
                                  .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis transakcija";
                excel.Workbook.Properties.Author = "FER-ZPR";
                var worksheet = excel.Workbook.Worksheets.Add("Transakcije");
                //First add the headers
                worksheet.Cells[1, 1].Value = "IdTransakcija";
                worksheet.Cells[1, 2].Value = "Ibanposiljatelja";
                worksheet.Cells[1, 3].Value = "Ibanprimatelja";
                worksheet.Cells[1, 4].Value = "OpisTransakcije";
                worksheet.Cells[1, 5].Value = "Iznos";
                worksheet.Cells[1, 6].Value = "Opis vrste transakcije";
                for (int i = 0; i < transakcija.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = transakcija[i].IdTransakcija;
                    worksheet.Cells[i + 2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[i + 2, 2].Value = transakcija[i].Ibanposiljatelja;
                    worksheet.Cells[i + 2, 3].Value = transakcija[i].Ibanprimatelja;
                    worksheet.Cells[i + 2, 4].Value = transakcija[i].OpisTransakcije;
                    worksheet.Cells[i + 2, 5].Value = transakcija[i].Iznos;
                    worksheet.Cells[i + 2, 6].Value = transakcija[i].IdVrstaTransakcijeNavigation.NazivTransakcije;
                }
                worksheet.Cells[1, 1, transakcija.Count + 1, 4].AutoFitColumns();
                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "transakcija.xlsx");
        }
        /// <summary>
        /// Stvara excel file popunjen podacima
        /// </summary>
        /// <returns>Vraca excel file</returns>
        public async Task<IActionResult> RacunExcel()
        {
            var racun = await ctx.RacunProjekta
                                  .AsNoTracking()
                                  .Include(d => d.IdProjektNavigation)
                                  .Include(d => d.IdTransakcijas)
                                  .OrderBy(d => d.Iban)
                                  .ToListAsync();
            var transakcija = await ctx.Transakcijas
                              .AsNoTracking()
                              .OrderBy(d => d.IdTransakcija)
                              .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis racuna projekata";
                excel.Workbook.Properties.Author = "FER-ZPR";
                var worksheet = excel.Workbook.Worksheets.Add("Racuni projekata");
                //First add the headers
                worksheet.Cells[1, 1].Value = "Iban";
                worksheet.Cells[1, 2].Value = "Naziv projekta";
                worksheet.Cells[1, 3].Value = "Stanje racuna";
                worksheet.Cells[1, 4].Value = "Id projekta";
                worksheet.Cells[1, 5].Value = "Opis transakcije";

                for (int i = 0; i < racun.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = racun[i].Iban;
                    worksheet.Cells[i + 2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[i + 2, 2].Value = racun[i].NazivProjekta;
                    worksheet.Cells[i + 2, 3].Value = racun[i].StanjeRacuna;
                    worksheet.Cells[i + 2, 4].Value = racun[i].IdProjekt;
                    // Inicijalizacija varijable za pohranu spojenih imena nositelja
                    string spojeniOpisi = "";
                    // Iteracija kroz zadatak listu
                    foreach (var ak in racun[i].IdTransakcijas)
                    {
                        spojeniOpisi += ak.OpisTransakcije + ", ";
                    }


                    // Ukloni zarez i razmak s kraja spojene varijable
                    spojeniOpisi = spojeniOpisi.TrimEnd(',', ' ');
                    // Postavljanje vrijednosti u ćeliju
                    worksheet.Cells[i + 2, 5].Value = spojeniOpisi;
                }
                worksheet.Cells[1, 1, racun.Count + 1, 4].AutoFitColumns();
                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "racuni.xlsx");
        }
        /// <summary>
        /// Stvara excel file popunjen master-detail podacima
        /// </summary>
        /// <param name="id">Id master podatka</param>
        /// <returns>Vraca excel file</returns>
        public async Task<IActionResult> MDRacunExcel(string id)
        {
            var racun = await ctx.RacunProjekta
                                  .AsNoTracking()
                                  .Include(d => d.IdProjektNavigation)
                                  .Include(d => d.IdTransakcijas)
                                  .ThenInclude(d => d.IdVrstaTransakcijeNavigation)
                                  .OrderBy(d => d.Iban)
                                  .ToListAsync();
            var transakcija = await ctx.Transakcijas
                              .AsNoTracking()
                              .Include(d => d.IdVrstaTransakcijeNavigation)
                              .OrderBy(d => d.IdTransakcija)
                              .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis racuna projekata";
                excel.Workbook.Properties.Author = "FER-ZPR";
                var worksheet = excel.Workbook.Worksheets.Add("Racuni");
                //First add the headers
                worksheet.Cells[1, 1].Value = "Iban";
                worksheet.Cells[1, 2].Value = "NazivProjekta";
                worksheet.Cells[1, 3].Value = "Stanje racuna";
                worksheet.Cells[1, 4].Value = "Id projekta";
                worksheet.Cells[1, 5].Value = "Opis transakcije";
                worksheet.Cells[1, 6].Value = "IdTransakcija";
                worksheet.Cells[1, 7].Value = "Ibanposiljatelja";
                worksheet.Cells[1, 8].Value = "Ibanprimatelja";
                worksheet.Cells[1, 9].Value = "OpisTransakcije";
                worksheet.Cells[1, 10].Value = "Iznos";
                worksheet.Cells[1, 11].Value = "Opis vrste transakcije";
                for (int i = 0; i < racun.Count; i++)
                {
                    if (racun[i].Iban == id)
                    {
                        worksheet.Cells[0 + 2, 1].Value = racun[i].Iban;
                        worksheet.Cells[0 + 2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        worksheet.Cells[0 + 2, 2].Value = racun[i].NazivProjekta;
                        worksheet.Cells[0 + 2, 3].Value = racun[i].StanjeRacuna;
                        worksheet.Cells[0 + 2, 4].Value = racun[i].IdProjekt;
                        // Dodajte novu petlju za prolazak kroz kolekciju projekata
                        // Iteracija kroz zadatak listu
                        int rowIndex = 3; // Početni redak za unos podataka o zadacima
                        foreach (var ak in racun[i].IdTransakcijas)
                        {
                            worksheet.InsertRow(rowIndex, 1);
                            worksheet.Cells[rowIndex - 1, 6].Value = ak.IdTransakcija;
                            worksheet.Cells[rowIndex - 1, 7].Value = ak.Ibanposiljatelja;
                            worksheet.Cells[rowIndex - 1, 8].Value = ak.Ibanprimatelja;
                            worksheet.Cells[rowIndex - 1, 9].Value = ak.OpisTransakcije;
                            worksheet.Cells[rowIndex - 1, 10].Value = ak.Iznos;
                            worksheet.Cells[rowIndex - 1, 11].Value = ak.IdVrstaTransakcijeNavigation.NazivTransakcije;
                            rowIndex++;
                        }




                    }
                }
                worksheet.Cells[1, 1, racun.Count + 1, 4].AutoFitColumns();
                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "master-detail-racun.xlsx");
        }
        /// <summary>
        /// Stvara excel file popunjen podacima
        /// </summary>
        /// <returns>Vraca excel file</returns>
        public async Task<IActionResult> DokumentExcel()
        {
            var dokument = await ctx.ProjektnaDokumentacijas
                                  .AsNoTracking()
                                  .Include(d => d.IdVrstaDokumentNavigation)
                                  .Include(d => d.IdProjektNavigation)
                                  .OrderBy(d => d.IdDokument)
                                  .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis dokumenata";
                excel.Workbook.Properties.Author = "FER-ZPR";
                var worksheet = excel.Workbook.Worksheets.Add("Dokumenti");

                //First add the headers
                worksheet.Cells[1, 1].Value = "IdDokument";
                worksheet.Cells[1, 2].Value = "Naziv";
                worksheet.Cells[1, 3].Value = "Vrsta";
                worksheet.Cells[1, 4].Value = "Format";
                worksheet.Cells[1, 5].Value = "Projekt";

                for (int i = 0; i < dokument.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = dokument[i].IdDokument;
                    worksheet.Cells[i + 2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[i + 2, 2].Value = dokument[i].NazivDokumenta;
                    worksheet.Cells[i + 2, 3].Value = dokument[i].IdVrstaDokumentNavigation.NazivVrste;
                    worksheet.Cells[i + 2, 4].Value = dokument[i].FormatDokumenta;
                    worksheet.Cells[i + 2, 5].Value = dokument[i].IdProjektNavigation.NazivProjekta;
                }

                worksheet.Cells[1, 1, dokument.Count + 1, 4].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "dokumenti.xlsx");
        }
        /// <summary>
        /// Stvara excel file popunjen podacima
        /// </summary>
        /// <returns>Vraca excel file</returns>
        public async Task<IActionResult> ProjektExcel()
        {
            var projekt = await ctx.Projekts
                                  .AsNoTracking()
                                  .Include(d => d.IdVrstaProjektaNavigation)
                                  .OrderBy(d => d.IdProjekt)
                                  .ToListAsync();
            var dokument = await ctx.ProjektnaDokumentacijas
                              .AsNoTracking()
                              .OrderBy(d => d.IdDokument)
                              .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis projekata";
                excel.Workbook.Properties.Author = "FER-ZPR";
                var worksheet = excel.Workbook.Worksheets.Add("Projekti");

                //First add the headers
                worksheet.Cells[1, 1].Value = "IdProjekta";
                worksheet.Cells[1, 2].Value = "Kratica";
                worksheet.Cells[1, 3].Value = "Naziv";
                worksheet.Cells[1, 4].Value = "Datum pocetka";
                worksheet.Cells[1, 5].Value = "Datum zavrsetka";
                worksheet.Cells[1, 6].Value = "Vrsta projekta";

                for (int i = 0; i < projekt.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = projekt[i].IdProjekt;
                    worksheet.Cells[i + 2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[i + 2, 2].Value = projekt[i].KraticaProjekta;
                    worksheet.Cells[i + 2, 3].Value = projekt[i].NazivProjekta;
                    worksheet.Cells[i + 2, 4].Value = projekt[i].DatumPocetka;
                    worksheet.Cells[i + 2, 5].Value = projekt[i].DatumZavrsetka;
                    worksheet.Cells[i + 2, 6].Value = projekt[i].IdVrstaProjektaNavigation.NazivVrsteProjekta;
                    string spojenaImena = "";


                }
                worksheet.Cells[1, 1, projekt.Count + 1, 4].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "projekti.xlsx");
        }
        /// <summary>
        /// Stvara excel file popunjen master-detail podacima
        /// </summary>
        /// <param name="id">Id master podatka</param>
        /// <returns>Vraca excel file</returns>
        public async Task<IActionResult> ProjektDokumentExcel(int id)
        {
            Console.WriteLine(id);
            var projekt = await ctx.Projekts
                                  .AsNoTracking()
                                  .Include(d => d.IdVrstaProjektaNavigation)
                                  .OrderBy(d => d.IdProjekt)
                                  .ToListAsync();
            var dokument = await ctx.ProjektnaDokumentacijas
                              .AsNoTracking()
                              .Include(d => d.IdVrstaDokumentNavigation)
                              .OrderBy(d => d.IdDokument)
                              .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis projekata";
                excel.Workbook.Properties.Author = "FER-ZPR";
                var worksheet = excel.Workbook.Worksheets.Add("Projekti");

                //First add the headers
                worksheet.Cells[1, 1].Value = "IdProjekt";
                worksheet.Cells[1, 2].Value = "Kratica";
                worksheet.Cells[1, 3].Value = "Naziv";
                worksheet.Cells[1, 4].Value = "Datum pocetka";
                worksheet.Cells[1, 5].Value = "Datum zavrsetka";
                worksheet.Cells[1, 6].Value = "Vrsta projekta";
                worksheet.Cells[1, 7].Value = "IdDokumenta";
                worksheet.Cells[1, 8].Value = "Naziv";
                worksheet.Cells[1, 9].Value = "Vrsta";
                worksheet.Cells[1, 10].Value = "Format";

                for (int i = 0; i < projekt.Count; i++)
                {
                    if (projekt[i].IdProjekt == id)
                    {
                        worksheet.Cells[0 + 2, 1].Value = projekt[i].IdProjekt;
                        worksheet.Cells[0 + 2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        worksheet.Cells[0 + 2, 2].Value = projekt[i].KraticaProjekta;
                        worksheet.Cells[0 + 2, 3].Value = projekt[i].NazivProjekta;
                        worksheet.Cells[0 + 2, 4].Value = projekt[i].DatumPocetka;
                        worksheet.Cells[0 + 2, 5].Value = projekt[i].DatumZavrsetka;
                        worksheet.Cells[0 + 2, 6].Value = projekt[i].IdVrstaProjektaNavigation.NazivVrsteProjekta;
                        int rowIndex = 3;


                        for (int k = 0; k < dokument.Count; k++)
                        {
                            if (dokument[k].IdProjekt == projekt[i].IdProjekt)
                            {
                                worksheet.InsertRow(rowIndex, 1);
                                worksheet.Cells[rowIndex - 1, 7].Value = dokument[k].IdDokument;
                                worksheet.Cells[rowIndex - 1, 8].Value = dokument[k].NazivDokumenta;
                                worksheet.Cells[rowIndex - 1, 9].Value = dokument[k].IdVrstaDokumentNavigation.NazivVrste;
                                worksheet.Cells[rowIndex - 1, 10].Value = dokument[k].FormatDokumenta;

                                rowIndex++;
                            }
                        }

                    }
                }
                worksheet.Cells[1, 1, projekt.Count + 1, 4].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "master-detail.xlsx");
        }
        /// <summary>
        /// Stvara excel file popunjen podacima
        /// </summary>
        /// <returns>Vraca excel file</returns>
        public async Task<IActionResult> AktivnostExcel()
        {
            var aktivnost = await ctx.Aktivnosts
                                  .AsNoTracking()
                                  .Include(d => d.IdVrstaAktivnostiNavigation)
                                  .OrderBy(d => d.IdAktivnost)
                                  .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis aktivnosti";
                excel.Workbook.Properties.Author = "FER-ZPR";
                var worksheet = excel.Workbook.Worksheets.Add("Aktivnosti");

                //First add the headers
                worksheet.Cells[1, 1].Value = "IdAktivnost";
                worksheet.Cells[1, 2].Value = "OpisAktivnosti";
                worksheet.Cells[1, 3].Value = "DatumPocetka";
                worksheet.Cells[1, 4].Value = "DatumZavrsetka";
                worksheet.Cells[1, 5].Value = "NazivVrsteAktivnosti";

                for (int i = 0; i < aktivnost.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = aktivnost[i].IdAktivnost;
                    worksheet.Cells[i + 2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[i + 2, 2].Value = aktivnost[i].OpisAktivnosti;
                    worksheet.Cells[i + 2, 3].Value = aktivnost[i].DatumPocetka;
                    worksheet.Cells[i + 2, 4].Value = aktivnost[i].DatumZavrsetka;
                    worksheet.Cells[i + 2, 5].Value = aktivnost[i].IdVrstaAktivnostiNavigation.NazivAktivnosti;
                }

                worksheet.Cells[1, 1, aktivnost.Count + 1, 4].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "aktivnost.xlsx");
        }
        /// <summary>
        /// Stvara excel file popunjen podacima
        /// </summary>
        /// <returns>Vraca excel file</returns>
        public async Task<IActionResult> PlanProjektaExcel()
        {
            var planProjekta = await ctx.PlanProjekta
                                  .AsNoTracking()
                                  .Include(d => d.IdProjektNavigation)
                                  .Include(d => d.IdVoditeljNavigation)
                                  .Include(d => d.IdAktivnosts)
                                  .OrderBy(d => d.IdPlanProjekta)
                                  .ToListAsync();
            var aktivnost = await ctx.Aktivnosts
                              .AsNoTracking()
                              .OrderBy(d => d.IdAktivnost)
                              .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis planova projekata";
                excel.Workbook.Properties.Author = "FER-ZPR";
                var worksheet = excel.Workbook.Worksheets.Add("Plan projekta");

                //First add the headers
                worksheet.Cells[1, 1].Value = "IdPlanProjekta";
                worksheet.Cells[1, 2].Value = "PlaniraniPocetakZadatka";
                worksheet.Cells[1, 3].Value = "StvarniPocetakZadatka";
                worksheet.Cells[1, 4].Value = "PlaniraniZavrsetakZadatka";
                worksheet.Cells[1, 5].Value = "StvarniZavrsetakZadatka";
                worksheet.Cells[1, 6].Value = "ImeVoditelja";
                worksheet.Cells[1, 7].Value = "NazivAktivnosti";
                worksheet.Cells[1, 8].Value = "ImeProjekta";

                for (int i = 0; i < planProjekta.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = planProjekta[i].IdPlanProjekta;
                    worksheet.Cells[i + 2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[i + 2, 2].Value = planProjekta[i].PlaniraniPocetakZadatka;
                    worksheet.Cells[i + 2, 3].Value = planProjekta[i].StvarniPocetakZadatka;
                    worksheet.Cells[i + 2, 4].Value = planProjekta[i].PlaniraniZavrsetakZadatka;
                    worksheet.Cells[i + 2, 5].Value = planProjekta[i].StvarniZavrsetakZadatka;
                    worksheet.Cells[i + 2, 6].Value = planProjekta[i].IdVoditeljNavigation.ImeVoditelja;

                    string spojeneAktivnosti = "";

                    foreach (var ak in planProjekta[i].IdAktivnosts)
                    {
                        spojeneAktivnosti += ak.OpisAktivnosti + ", ";
                    }

                    // Ukloni zarez i razmak s kraja spojene varijable
                    spojeneAktivnosti = spojeneAktivnosti.TrimEnd(',', ' ');

                    // Postavljanje vrijednosti u ćeliju za projekte
                    worksheet.Cells[i + 2, 7].Value = spojeneAktivnosti;

                    // Postavljanje vrijednosti u ćeliju za projekte
                    worksheet.Cells[i + 2, 8].Value = planProjekta[i].IdProjektNavigation.NazivProjekta;
                }
                worksheet.Cells[1, 1, planProjekta.Count + 1, 4].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "planProjekta.xlsx");
        }
        /// <summary>
        /// Stvara excel file popunjen master-detail podacima
        /// </summary>
        /// <param name="id">Id master podatka</param>
        /// <returns>Vraca excel file</returns>
        public async Task<IActionResult> MDPlanProjektaExcel(int id)
        {
            var planProjekta = await ctx.PlanProjekta
                                  .AsNoTracking()
                                  .Include(d => d.IdProjektNavigation)
                                  .Include(d => d.IdVoditeljNavigation)
                                  .Include(d => d.IdAktivnosts)
                                  .OrderBy(d => d.IdPlanProjekta)
                                  .ToListAsync();
            var aktivnost = await ctx.Aktivnosts
                                  .AsNoTracking()
                                  .Include(d => d.IdVrstaAktivnostiNavigation)
                                  .OrderBy(d => d.IdAktivnost)
                                  .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis planova projekata";
                excel.Workbook.Properties.Author = "FER-ZPR";
                var worksheet = excel.Workbook.Worksheets.Add("PlanProjekta");

                //First add the headers
                worksheet.Cells[1, 1].Value = "IdPlanProjekta";
                worksheet.Cells[1, 2].Value = "PlaniraniPocetakZadatka";
                worksheet.Cells[1, 3].Value = "StvarniPocetakZadatka";
                worksheet.Cells[1, 4].Value = "PlaniraniZavrsetakZadatka";
                worksheet.Cells[1, 5].Value = "StvarniZavrsetakZadatka";
                worksheet.Cells[1, 6].Value = "ImeVoditelja";
                worksheet.Cells[1, 7].Value = "NazivAktivnosti";
                worksheet.Cells[1, 8].Value = "ImeProjekta";
                worksheet.Cells[1, 9].Value = "IdAktivnost";
                worksheet.Cells[1, 10].Value = "OpisAktivnosti";
                worksheet.Cells[1, 11].Value = "DatumPocetka";
                worksheet.Cells[1, 12].Value = "DatumZavrsetka";
                worksheet.Cells[1, 13].Value = "NazivVrsteAktivnosti";

                for (int i = 0; i < planProjekta.Count; i++)
                {
                    if (planProjekta[i].IdPlanProjekta == id)
                    {
                        worksheet.Cells[0 + 2, 1].Value = planProjekta[i].IdPlanProjekta;
                        worksheet.Cells[0 + 2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        worksheet.Cells[0 + 2, 2].Value = planProjekta[i].PlaniraniPocetakZadatka;
                        worksheet.Cells[0 + 2, 3].Value = planProjekta[i].StvarniPocetakZadatka;
                        worksheet.Cells[0 + 2, 4].Value = planProjekta[i].PlaniraniZavrsetakZadatka;
                        worksheet.Cells[0 + 2, 5].Value = planProjekta[i].StvarniZavrsetakZadatka;
                        worksheet.Cells[0 + 2, 6].Value = planProjekta[i].IdVoditeljNavigation.ImeVoditelja;

                        string spojeneAktivnosti = "";

                        foreach (var ak in planProjekta[i].IdAktivnosts)
                        {
                            spojeneAktivnosti += ak.OpisAktivnosti + ", ";
                        }

                        // Ukloni zarez i razmak s kraja spojene varijable
                        spojeneAktivnosti = spojeneAktivnosti.TrimEnd(',', ' ');

                        // Postavljanje vrijednosti u ćeliju za projekte
                        worksheet.Cells[0 + 2, 7].Value = spojeneAktivnosti;

                        // Postavljanje vrijednosti u ćeliju za projekte
                        worksheet.Cells[0 + 2, 8].Value = planProjekta[i].IdProjektNavigation.NazivProjekta;


                        int rowIndex = 3; // Početni redak za unos podataka o zadacima

                        foreach (var ak in planProjekta[i].IdAktivnosts)
                        {
                            worksheet.InsertRow(rowIndex, 1);
                            worksheet.Cells[rowIndex - 1, 9].Value = ak.IdAktivnost;
                            worksheet.Cells[rowIndex - 1, 10].Value = ak.OpisAktivnosti;
                            worksheet.Cells[rowIndex - 1, 11].Value = ak.DatumPocetka;
                            worksheet.Cells[rowIndex - 1, 12].Value = ak.DatumZavrsetka;
                            worksheet.Cells[rowIndex - 1, 13].Value = ak.IdVrstaAktivnosti;

                            rowIndex++; // Povećajte indeks retka za sljedeći zadatak
                        }



                    }
                }
                worksheet.Cells[1, 1, planProjekta.Count + 1, 4].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "master-detail.xlsx");
        }
        /// <summary>
        /// Stvara excel file popunjen podacima
        /// </summary>
        /// <returns>Vraca excel file</returns>
        public async Task<IActionResult> UlogaExcel()
        {
            var uloga = await ctx.Uloges
                                  .AsNoTracking()
                                  .Include(u => u.IdVrstaUlogaNavigation)
                                  .OrderBy(u => u.IdUloga)
                                  .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis uloga";
                excel.Workbook.Properties.Author = "FER-ZPR";
                var worksheet = excel.Workbook.Worksheets.Add("Uloge");

                //First add the headers
                worksheet.Cells[1, 1].Value = "IdUloga";
                worksheet.Cells[1, 2].Value = "VrstaUloga";
                worksheet.Cells[1, 3].Value = "OpisUloga";

                for (int i = 0; i < uloga.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = uloga[i].IdUloga;
                    worksheet.Cells[i + 2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[i + 2, 2].Value = uloga[i].IdVrstaUlogaNavigation.Naziv;
                    worksheet.Cells[i + 2, 3].Value = uloga[i].OpisUloga;
                }

                worksheet.Cells[1, 1, uloga.Count + 1, 4].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "uloga.xlsx");
        }
        /// <summary>
        /// Stvara excel file popunjen podacima
        /// </summary>
        /// <returns>Vraca excel file</returns>
        public async Task<IActionResult> SudionikExcel()
        {
            var sudionik = await ctx.Sudioniks
                                  .AsNoTracking()
                                  .Include(s => s.IdFirmaNavigation)
                                  .OrderBy(s => s.IdSudionik)
                                  .ToListAsync();
            var pripadnostsFirmi = await ctx.PripadnostFirmis
                              .AsNoTracking()
                              .OrderBy(p => p.IdFirma)
                              .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis sudionika";
                excel.Workbook.Properties.Author = "FER-ZPR";
                var worksheet = excel.Workbook.Worksheets.Add("Sudionici");

                //First add the headers
                worksheet.Cells[1, 1].Value = "IdSudionik";
                worksheet.Cells[1, 2].Value = "Email";
                worksheet.Cells[1, 3].Value = "Kontakt";
                worksheet.Cells[1, 4].Value = "Adresa ureda";
                worksheet.Cells[1, 5].Value = "Frima";
                worksheet.Cells[1, 6].Value = "Adresa sjedista";

                for (int i = 0; i < sudionik.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = sudionik[i].IdSudionik;
                    worksheet.Cells[i + 2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[i + 2, 2].Value = sudionik[i].Email;
                    worksheet.Cells[i + 2, 3].Value = sudionik[i].Kontakt;
                    worksheet.Cells[i + 2, 4].Value = sudionik[i].AdresaUreda;
                    worksheet.Cells[i + 2, 5].Value = sudionik[i].IdFirmaNavigation.NazivFirma;
                    worksheet.Cells[i + 2, 6].Value = sudionik[i].IdFirmaNavigation.AdresaSjedista;
                }
                worksheet.Cells[1, 1, sudionik.Count + 1, 4].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "sudionik.xlsx");
        }
        /// <summary>
        /// Stvara excel file popunjen master-detail podacima
        /// </summary>
        /// <param name="id">Id master podatka</param>
        /// <returns>Vraca excel file</returns>
        public async Task<IActionResult> MDSudionikExcel(int id)
        {
            var sudjeluju = await ctx.Sudjelujus
                                  .AsNoTracking()
                                  .Include(s => s.IdProjektNavigation)
                                  .Include(s => s.IdUlogaNavigation)
                                  .Include(s => s.IdUlogaNavigation).ThenInclude(s => s.IdVrstaUlogaNavigation)
                                  .OrderBy(s => s.IdSudionik)
                                  .ToListAsync();
            var sudionik = await ctx.Sudioniks
                              .AsNoTracking()
                              .Include(d => d.IdFirmaNavigation)
                              .OrderBy(d => d.IdSudionik)
                              .ToListAsync();
            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Popis sudjelovanja sudionika";
                excel.Workbook.Properties.Author = "FER-ZPR";
                var worksheet = excel.Workbook.Worksheets.Add("Sudionik");

                //First add the headers
                worksheet.Cells[1, 1].Value = "IdSudionik";
                worksheet.Cells[1, 2].Value = "Email";
                worksheet.Cells[1, 3].Value = "Kontakt";
                worksheet.Cells[1, 4].Value = "AdresaUreda";
                worksheet.Cells[1, 5].Value = "NazivFirma";
                worksheet.Cells[1, 6].Value = "Projekt";
                worksheet.Cells[1, 7].Value = "Uloga";
                worksheet.Cells[1, 8].Value = "Vrsta uloge";

                for (int i = 0; i < sudionik.Count; i++)
                {
                    if (sudionik[i].IdSudionik == id)
                    {
                        worksheet.Cells[0 + 2, 1].Value = sudionik[i].IdSudionik;
                        worksheet.Cells[0 + 2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        worksheet.Cells[0 + 2, 2].Value = sudionik[i].Email;
                        worksheet.Cells[0 + 2, 3].Value = sudionik[i].Kontakt;
                        worksheet.Cells[0 + 2, 4].Value = sudionik[i].IdFirmaNavigation.AdresaSjedista;
                        worksheet.Cells[0 + 2, 5].Value = sudionik[i].IdFirmaNavigation.NazivFirma;

                        int rowIndex = 3;
                        for (int k = 0; k < sudjeluju.Count; k++)
                        {
                            if (sudjeluju[k].IdSudionik == sudionik[i].IdSudionik)
                            {
                                worksheet.InsertRow(rowIndex, 1);
                                worksheet.Cells[rowIndex - 1, 6].Value = sudjeluju[k].IdProjektNavigation.NazivProjekta;
                                worksheet.Cells[rowIndex - 1, 7].Value = sudjeluju[k].IdUlogaNavigation.OpisUloga;
                                worksheet.Cells[rowIndex - 1, 8].Value = sudjeluju[k].IdUlogaNavigation.IdVrstaUlogaNavigation.Naziv;

                                rowIndex++;
                            }
                        }

                    }
                }
                worksheet.Cells[1, 1, sudionik.Count + 1, 4].AutoFitColumns();

                content = excel.GetAsByteArray();
            }
            return File(content, ExcelContentType, "master-detail-sudionik.xlsx");
        }
    }
}
