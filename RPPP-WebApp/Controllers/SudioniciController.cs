using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using RPPP_WebApp;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Extensions.Selectors;
using Microsoft.AspNetCore.Mvc.Rendering;
using NLog.Fluent;

namespace RPPP_WebApp.Controllers
{
    public class SudionikController : Controller
    {
        private readonly ProjektContext ctx;
        private readonly ILogger<SudionikController> logger;
        private readonly AppSettings appSettings;

        public SudionikController(ProjektContext ctx, IOptionsSnapshot<AppSettings> options, ILogger<SudionikController> logger)
        {
            this.ctx = ctx;
            this.logger = logger;
            appSettings = options.Value;
        }
        /// <summary>
        /// Prikaz stranice
        /// </summary>
        /// <param name="page">Broj stranice</param>
        /// <param name="sort">Vrijednost po kojoj se sortira</param>
        /// <param name="ascending">Redoslijed sortiranja</param>
        /// <returns>Vraca prikaz stranice</returns>
        public IActionResult Index(int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;

            var query = ctx.Sudioniks.AsNoTracking();

            int count = query.Count();
            if (count == 0)
            {
                logger.LogInformation("Ne postoji nijedan sudionik");
                TempData[Constants.Message] = "Ne postoji niti jedan sudionik.";
                TempData[Constants.ErrorOccurred] = false;
                return RedirectToAction(nameof(Create));
            }

            var pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                Sort = sort,
                Ascending = ascending,
                ItemsPerPage = pagesize,
                TotalItems = count
            };
            if (page < 1 || page > pagingInfo.TotalPages)
            {
                return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
            }

            query = query.ApplySort(sort, ascending);

            var sudionici = query
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .Include(s => s.IdFirmaNavigation)
                .Include(s => s.Sudjelujus)
                .ThenInclude(u => u.IdUlogaNavigation)
                .ToList();

            var model = new SudioniciViewModel
            {
                Sudionici = sudionici,
                PagingInfo = pagingInfo
            };

            return View(model);
        }
        /// <summary>
        /// Stvaranje podatka
        /// </summary>
        /// <returns>Prikaz forme sa dropdown listom</returns>
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PrepareDropDownLists();
            return View();
        }
        /// <summary>
        /// Stvaranje novog podatka
        /// </summary>
        /// <param name="model">Sluzi za stvaranje parametara forme</param>
        /// <returns>Vraca rezultat stvaranja novog podatka</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(SudionikViewModel model)
        {
            logger.LogTrace(JsonSerializer.Serialize(model));

            if (ModelState.IsValid)
            {
                try
                {

                    var sudionik = new Sudionik
                    {
                        IdSudionik = model.IdSudionik,
                        Email = model.Email,
                        Kontakt = model.Kontakt,
                        AdresaUreda = model.AdresaUreda,
                        IdFirma = model.IdFirma
                    };

                    var sudjeluju = new Sudjeluju
                    {
                        IdProjekt = model.IdProjekt,
                        IdUloga = model.IdUloga,
                        IdSudionik = model.IdSudionik
                    };

                    using (ctx)
                    {
                        ctx.Sudioniks.Add(sudionik);
                        ctx.Sudjelujus.Add(sudjeluju);
                        ctx.SaveChanges();
                    }

                    TempData[Constants.Message] = "Sudionik uspješno dodan.";
                    TempData[Constants.ErrorOccurred] = false;

                    return RedirectToAction(nameof(Index));

                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanja novog sudionika: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                }
            }

            return View(model);
        }

        /// <summary>
        /// Metoda za brisanje.
        /// </summary>
        /// <param name="id">Id podatka koji se brise</param>
        /// <returns>Vraca trenutnu stranicu nakon brisanja</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var sudionik = ctx.Sudioniks.Find(id);

            if (sudionik != null)
            {
                try
                {
                    var sudjelujuRecords = ctx.Sudjelujus.Where(s => s.IdSudionik == id);
                    ctx.Sudjelujus.RemoveRange(sudjelujuRecords);

                    ctx.Sudioniks.Remove(sudionik);
                    ctx.SaveChanges();

                    TempData[Constants.Message] = "Sudionik uspješno obrisan.";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = "Pogreška prilikom brisanja Sudionika: " + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                }
            }
            else
            {
                TempData[Constants.Message] = "Sudionik nije pronađen.";
                TempData[Constants.ErrorOccurred] = true;
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Uredivanje podatka
        /// </summary>
        /// <param name="id">Id podatka koji se ureduje</param>
        /// <param name="page">Stranica na kojoj se nalazimo</param>
        /// <param name="sort">Vrijednost po kojoj se sortira</param>
        /// <param name="ascending">Redoslijed sortiranja</param>
        /// <returns>Vraca formu sa vrijednostima uredenog podatka</returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id, int page, int sort, bool ascending = true)
        {
            var sudionik = await ctx.Sudioniks
                                .Where(d => d.IdSudionik == id)
                                .Select(d => new SudionikViewModel
                                {
                                    IdSudionik = d.IdSudionik,
                                    Email = d.Email,
                                    Kontakt = d.Kontakt,
                                    AdresaUreda = d.AdresaUreda,
                                    IdFirma = d.IdFirma,

                                })
                                .FirstOrDefaultAsync();
            if (sudionik == null)
            {
                return NotFound($"Sudionik s {id} ne postoji");
            }
            else
            {
                var sudjeluju = await ctx.Sudjelujus
                                        .Where(s => s.IdSudionik == sudionik.IdSudionik)
                                        .OrderBy(s => s.IdSudionik)
                                        .Select(s => new Sudjeluju
                                        {
                                            IdProjekt = s.IdProjekt,
                                            IdUloga = s.IdUloga,
                                            IdSudionik = s.IdSudionik,
                                            IdSudionikNavigation = s.IdSudionikNavigation,
                                            IdProjektNavigation = s.IdProjektNavigation,
                                            IdUlogaNavigation = s.IdUlogaNavigation,
                                        })
                                        .ToListAsync();
                sudionik.Sudjelujus = sudjeluju;

                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                await PrepareDropDownLists();
                return View(sudionik);

            }
        }
        /// <summary>
        /// Spremanje promjena uredivanog podatka
        /// </summary>
        /// <param name="model">Model po kojem se stvaraju parametri za uredivanje podatka</param>
        /// <param name="page">Stranica na kojoj se nalazimo</param>
        /// <param name="sort">Vrijednost po kojoj se sortira</param>
        /// <param name="ascending">Redoslijed sortiranja</param>
        /// <returns>Vraca promijenjeni podatak</returns>
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(SudionikViewModel model, int page, int sort, bool ascending = true)
        {

            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;
            if (ModelState.IsValid)
            {
                var sudionik = await ctx.Sudioniks
                                        .Include(d => d.Sudjelujus)
                                        .Where(d => d.IdSudionik == model.IdSudionik)
                                        .FirstOrDefaultAsync();
                if (sudionik == null)
                {
                    return NotFound("Ne postoji sudionik s id-om: " + model.IdSudionik);
                }

                sudionik.IdSudionik = model.IdSudionik;
                sudionik.Email = model.Email;
                sudionik.Kontakt = model.Kontakt;
                sudionik.AdresaUreda = model.AdresaUreda;
                sudionik.IdFirma = model.IdFirma;
                try
                {

                    await ctx.SaveChangesAsync();

                    TempData[Constants.Message] = $"Sudionik {sudionik.IdSudionik} uspješno ažuriran.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Edit), new
                    {
                        id = sudionik.IdSudionik,
                        page,
                        sort,
                        ascending
                    });

                }
                catch (Exception exc)
                {
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(model);
                }
            }
            else
            {
                return View(model);
            }
        }

        private async Task PrepareDropDownLists()
        {
            var pripadnostFirmi = await ctx.PripadnostFirmis
                                .OrderBy(p => p.IdFirma)
                                .Select(p => new { p.IdFirma, p.NazivFirma })
                                .ToListAsync();

            ViewBag.PripadnostFirmi = new SelectList(pripadnostFirmi, nameof(PripadnostFirmi.IdFirma), nameof(PripadnostFirmi.NazivFirma));

            var projekt = await ctx.Projekts
                                .OrderBy(p => p.IdProjekt)
                                .Select(p => new { p.IdProjekt, p.NazivProjekta })
                                .ToListAsync();

            ViewBag.Projekt = new SelectList(projekt, nameof(Projekt.IdProjekt), nameof(Projekt.NazivProjekta));

            var uloga = await ctx.Uloges
                                .OrderBy(u => u.IdUloga)
                                .Select(u => new { u.IdUloga, u.OpisUloga })
                                .ToListAsync();

            ViewBag.Uloga = new SelectList(uloga, nameof(Uloge.IdUloga), nameof(Uloge.OpisUloga));
        }
        /// <summary>
        /// Master-detail prikaz
        /// </summary>
        /// <param name="id">Id podatka koji se prikazuje</param>
        /// <param name="page">Stranica na kojoj se nalazimo</param>
        /// <param name="sort">Vrijednost po kojoj se sortira</param>
        /// <param name="ascending">Redoslijed sortiranja</param>
        /// <returns>Vraca master-detail prikaz podataka</returns>
        public IActionResult STP(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            int pagesize = appSettings.PageSize;

            var query = ctx.Sudioniks.AsNoTracking().Where(s => s.IdSudionik == id);

            int count = query.Count();

            var pagingInfo = new PagingInfo
            {
                CurrentPage = page,
                Sort = sort,
                Ascending = ascending,
                ItemsPerPage = pagesize,
                TotalItems = count
            };
            if (page < 1 || page > pagingInfo.TotalPages)
            {
                return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
            }

            var sudjeluju = query
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .Include(s => s.Sudjelujus)
                .ThenInclude(p => p.IdProjektNavigation)
                .Include(s => s.Sudjelujus)
                .ThenInclude(u => u.IdUlogaNavigation)
                .ThenInclude(vu => vu.IdVrstaUlogaNavigation)
                .Include(s => s.IdFirmaNavigation)
                .ToList();

            var model = new SudioniciViewModel
            {
                Sudionici = sudjeluju,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

    }
}