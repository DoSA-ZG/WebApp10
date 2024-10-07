using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RPPP_WebApp.Extensions.Selectors;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace RPPP_WebApp.Controllers
{
    public class ZahtjevController : Controller
    {
        private readonly ProjektContext ctx;
        private readonly ILogger<ZahtjevController> logger;
        private readonly AppSettings appSettings;

        public ZahtjevController(ProjektContext ctx, IOptionsSnapshot<AppSettings> options, ILogger<ZahtjevController> logger)
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

            var query = ctx.Zahtjevs.AsNoTracking();

            int count = query.Count();
            if (count == 0)
            {
                logger.LogInformation("Ne postoji nijedan zahtjev");
                TempData[RPPP_WebApp.Constants.Message] = "Ne postoji niti jedan zahtjev.";
                TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;
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

            var zahtjevi = query
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .Include(z => z.Zadacis)
                .Include(p => p.IdProjekts)
                .ToList();

            var model = new ZahtjeviViewModel
            {
                Zahtjev = zahtjevi,
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
        public IActionResult Create(ZahtjevViewModel model)
        {
            logger.LogTrace(JsonSerializer.Serialize(model));
            if (ModelState.IsValid)
            {
                try
                {
                    var projekt = ctx.Projekts.Find(model.IdProjekt);
                    var zahtjev = new Zahtjev();
                    zahtjev.IdZahtjev = model.IdZahtjev;
                    zahtjev.Opis = model.Opis;
                    zahtjev.Vrsta = model.Vrsta;
                    zahtjev.Prioritet = model.Prioritet;
                    zahtjev.IdProjekts.Add(projekt);
                    ctx.Add(zahtjev);
                    ctx.SaveChanges();
                    logger.LogInformation(new EventId(1000), $"Zahtjev {zahtjev.Opis} dodan.");

                    TempData[RPPP_WebApp.Constants.Message] = $"Zahtjev {zahtjev.Opis} dodan.";
                    TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanje novog zahtjeva: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(model);
                }
            }
            else
            {
                return View(model);
            }
        }
        /// <summary>
        /// Metoda za brisanje.
        /// </summary>
        /// <param name="id">Id podatka koji se brise</param>
        /// <param name="page">Stranica na kojoj se nalazimo</param>
        /// <param name="sort">Vrijednost po kojoj se sortira</param>
        /// <param name="ascending">Redoslijed sortiranja</param>
        /// <returns>Vraca trenutnu stranicu nakon brisanja</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, int page, int sort, bool ascending = true)
        {
            var zahtjev = ctx.Zahtjevs.Find(id);

            if (zahtjev != null)
            {
                try
                {

                    foreach (var projekt in ctx.Projekts.Include(p => p.IdZahtjevs))
                    {
                        var zahtjevZaBrisanje = projekt.IdZahtjevs.FirstOrDefault(z => z.IdZahtjev == id);
                        if (zahtjevZaBrisanje != null)
                        {
                            projekt.IdZahtjevs.Remove(zahtjevZaBrisanje);
                            break;
                        }
                    }


                    ctx.Remove(zahtjev);
                    ctx.SaveChanges();

                    logger.LogInformation($"Zahtjev uspješno obrisan");
                    TempData[RPPP_WebApp.Constants.Message] = $"Zahtjev uspješno obrisan";
                    TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[RPPP_WebApp.Constants.Message] = "Pogreška prilikom brisanja zahtjeva: " + exc.CompleteExceptionMessage();
                    TempData[RPPP_WebApp.Constants.ErrorOccurred] = true;
                    logger.LogError("Pogreška prilikom brisanja zahtjeva: " + exc.CompleteExceptionMessage());
                }
            }
            else
            {
                logger.LogWarning("Ne postoji zahtjev s ID-om: {0} ", id);
                TempData[RPPP_WebApp.Constants.Message] = "Ne postoji zahtjev s ID-om: " + id;
                TempData[RPPP_WebApp.Constants.ErrorOccurred] = true;
            }
            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
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
            var zahtjev = await ctx.Zahtjevs
                                .Where(d => d.IdZahtjev == id)
                                .Select(d => new ZahtjevViewModel
                                {
                                    IdZahtjev = d.IdZahtjev,
                                    Opis = d.Opis,
                                    Prioritet = d.Prioritet,
                                    Vrsta = d.Vrsta

                                })
                                .FirstOrDefaultAsync();
            if (zahtjev == null)
            {
                return NotFound($"Dokument {id} ne postoji");
            }
            else
            {
                //uèitavanje stavki
                var zadaci = await ctx.Zadacis
                                      .Where(s => s.IdZahtjev == zahtjev.IdZahtjev)
                                      .OrderBy(s => s.IdZadatak)
                                      .Select(s => new ZadatakViewModel
                                      {
                                          IdZadatak = s.IdZadatak,
                                          Status = s.Status,
                                          Aktivan = s.Aktivan,
                                          Opis = s.Opis,
                                          IdZahtjev = s.IdZahtjev,
                                          NositeljZadatka = s.NositeljZadatka,
                                          IdPrioritetZadatka = (int)s.IdPrioritetZadatka
                                      })
                                      .ToListAsync();
                zahtjev.Zadacis = zadaci;

                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;

                return View(zahtjev);

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
        public async Task<IActionResult> Update(ZahtjevViewModel model, int page, int sort, bool ascending = true)
        {

            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;
            if (ModelState.IsValid)
            {
                var zahtjev = await ctx.Zahtjevs
                                        .Include(d => d.Zadacis)
                                        .Where(d => d.IdZahtjev == model.IdZahtjev)
                                        .FirstOrDefaultAsync();
                if (zahtjev == null)
                {
                    return NotFound("Ne postoji zahtjev s id-om: " + model.IdZahtjev);
                }

                zahtjev.IdZahtjev = model.IdZahtjev;
                zahtjev.Opis = model.Opis;
                zahtjev.Prioritet = model.Prioritet;
                zahtjev.Vrsta = model.Vrsta;
                try
                {

                    await ctx.SaveChangesAsync();

                    TempData[Constants.Message] = $"Zahtjev {zahtjev.IdZahtjev} uspješno ažuriran.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Edit), new
                    {
                        id = zahtjev.IdZahtjev,
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

            var query = ctx.Zahtjevs.AsNoTracking().Where(z => z.IdZahtjev == id);

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

            var zadaci = query
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .Include(z => z.Zadacis)
                .ToList();

            var model = new ZahtjeviViewModel
            {
                Zahtjev = zadaci,
                PagingInfo = pagingInfo
            };

            return View(model);
        }
        private async Task PrepareDropDownLists()
        {

            var projekti = await ctx.Projekts
                                 .OrderBy(p => p.IdProjekt)
                                 .Select(p => new { p.IdProjekt, p.NazivProjekta })
                                 .ToListAsync();

            ViewBag.Projekti = new SelectList(projekti, nameof(Projekt.IdProjekt), nameof(Projekt.NazivProjekta));
        }

    }
}