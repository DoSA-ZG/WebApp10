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
    public class PlanProjektaController : Controller
    {
        private readonly ProjektContext ctx;
        private readonly ILogger<PlanProjektaController> logger;
        private readonly AppSettings appSettings;

        public PlanProjektaController(ProjektContext ctx, IOptionsSnapshot<AppSettings> options, ILogger<PlanProjektaController> logger)
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

            var query = ctx.PlanProjekta.AsNoTracking();

            int count = query.Count();
            if (count == 0)
            {
                logger.LogInformation("Ne postoji nijedan plan projekta");
                TempData[RPPP_WebApp.Constants.Message] = "Ne postoji niti jedan plan projekta.";
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

            var planoviProjekta = query
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .Include(p => p.IdAktivnosts)
                .Include(p => p.IdProjektNavigation)
                .Include(p => p.IdVoditeljNavigation)
                .ToList();

            var model = new PlanProjektaViewModel
            {
                PlanoviProjekta = planoviProjekta,
                PagingInfo = pagingInfo
            };

            return View(model);
        }
        /// <summary>
        /// Stvaranje aktivnosti
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
        public IActionResult Create(PlanProjekataViewModel model)
        {
            logger.LogTrace(JsonSerializer.Serialize(model));
            if (ModelState.IsValid)
            {
                try
                {
                    var planProjekta = new PlanProjektum();
                    planProjekta.IdPlanProjekta = model.IdPlanProjekta;
                    planProjekta.PlaniraniPocetakZadatka = model.PlaniraniPocetakZadatka;
                    planProjekta.StvarniPocetakZadatka = model.StvarniPocetakZadatka;
                    planProjekta.PlaniraniZavrsetakZadatka = model.PlaniraniZavrsetakZadatka;
                    planProjekta.StvarniZavrsetakZadatka = model.StvarniZavrsetakZadatka;
                    planProjekta.IdProjekt = model.IdProjekt;
                    planProjekta.IdVoditelj = model.IdVoditelj;
                    ctx.Add(planProjekta);
                    ctx.SaveChanges();
                    logger.LogInformation(new EventId(1000), $"Plan projekta {planProjekta.IdPlanProjekta} dodan.");

                    TempData[RPPP_WebApp.Constants.Message] = $"Plan projekta {planProjekta.IdPlanProjekta} dodan.";
                    TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanja novog plana projekta: {0}", exc.CompleteExceptionMessage());
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
        /// <returns>Vraca trenutnu stranicu nakon brisanja</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                var planProjekta = ctx.PlanProjekta
                    .Include(p => p.IdAktivnosts)  // Uključite podatke o povezanim aktivnostima
                    .FirstOrDefault(p => p.IdPlanProjekta == id);

                if (planProjekta != null)
                {
                    foreach (var aktivnost in ctx.Aktivnosts.Include(p => p.IdPlanProjekta))
                    {
                        var zahtjevZaBrisanje = aktivnost.IdPlanProjekta.FirstOrDefault(z => z.IdPlanProjekta == id);
                        if (zahtjevZaBrisanje != null)
                        {
                            aktivnost.IdPlanProjekta.Remove(zahtjevZaBrisanje);
                            //break;
                        }
                    }

                    // Uklonite povezane aktivnosti iz plana projekta
                    planProjekta.IdAktivnosts.Clear();

                    ctx.Remove(planProjekta);
                    ctx.SaveChanges();

                    logger.LogInformation($"Plan projekta {planProjekta.IdPlanProjekta} uspješno obrisan.");
                    TempData[RPPP_WebApp.Constants.Message] = $"Plan projekta {planProjekta.IdPlanProjekta} uspješno obrisan.";
                    TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;
                }
                else
                {
                    logger.LogWarning("Ne postoji plan projekta s ID-om: {0} ", id);
                    TempData[RPPP_WebApp.Constants.Message] = $"Ne postoji plan projekta s ID-om: {id}.";
                    TempData[RPPP_WebApp.Constants.ErrorOccurred] = true;
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception exc)
            {
                logger.LogError("Pogreška prilikom brisanja plana projekta: {0}", exc.CompleteExceptionMessage());
                TempData[RPPP_WebApp.Constants.Message] = $"Pogreška prilikom brisanja plana projekta: {exc.CompleteExceptionMessage()}";
                TempData[RPPP_WebApp.Constants.ErrorOccurred] = true;

                return RedirectToAction(nameof(Index));
            }
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
            var planProjekta = await ctx.PlanProjekta
                                .Where(d => d.IdPlanProjekta == id)
                                .Select(d => new PlanProjekataViewModel
                                {
                                    IdPlanProjekta = d.IdPlanProjekta,
                                    PlaniraniPocetakZadatka = d.PlaniraniPocetakZadatka,
                                    StvarniPocetakZadatka = d.StvarniPocetakZadatka,
                                    PlaniraniZavrsetakZadatka = d.PlaniraniZavrsetakZadatka,
                                    StvarniZavrsetakZadatka = d.StvarniZavrsetakZadatka,
                                    IdProjekt = d.IdProjekt,
                                    IdVoditelj = d.IdVoditelj,
                                })
                                .FirstOrDefaultAsync();
            if (planProjekta == null)
            {
                return NotFound($"Dokument {id} ne postoji");
            }
            else
            {
                //učitavanje stavki
                var aktivnost = await ctx.Aktivnosts
                                      .OrderBy(s => s.IdAktivnost)
                                      .Select(s => new AktivnostiViewModel
                                      {
                                          IdAktivnost = s.IdAktivnost,
                                          OpisAktivnosti = s.OpisAktivnosti,
                                          DatumPocetka = s.DatumPocetka,
                                          DatumZavrsetka = s.DatumZavrsetka,
                                          IdVrstaAktivnosti = s.IdVrstaAktivnosti
                                      })
                                      .ToListAsync();
                var planProjektaAktivnosti = await ctx.PlanProjekta
                                             .Where(d => d.IdPlanProjekta == id)
                                             .Select(d => d.IdAktivnosts)
                                             .ToListAsync();
                var newAktivnost = aktivnost.ToList();
                List<int> aktivnostIdList = new List<int>();
                foreach (var item in planProjektaAktivnosti)
                {
                    foreach (var item2 in item)
                    {
                        aktivnostIdList.Add(item2.IdAktivnost);
                    }
                }
                foreach (var item in newAktivnost)
                {
                    if (aktivnostIdList.Contains(item.IdAktivnost) == false)
                    {
                        aktivnost.Remove(item);
                    }
                }
                planProjekta.IdAktivnosts = aktivnost;

                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;

                return View(planProjekta);

            }
        }
        /// <summary>
        /// Spremanje promjena uredivanog podatka
        /// </summary>
        /// <param name="id">Id podatka koji se ureduje</param>
        /// <param name="page">Stranica na kojoj se nalazimo</param>
        /// <param name="sort">Vrijednost po kojoj se sortira</param>
        /// <param name="ascending">Redoslijed sortiranja</param>
        /// <returns>Vraca promijenjeni podatak</returns>
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, int page, int sort, bool ascending = true)
        {
            try
            {
                PlanProjektum planProjekta = await ctx.PlanProjekta
                                  .Where(p => p.IdPlanProjekta == id)
                                  .FirstOrDefaultAsync();
                if (planProjekta == null)
                {
                    return NotFound("Neispravan ID plana projekta: " + id);
                }

                if (await TryUpdateModelAsync<PlanProjektum>(planProjekta, "",
                   p => p.PlaniraniPocetakZadatka, p => p.StvarniPocetakZadatka,
                   p => p.PlaniraniZavrsetakZadatka, p => p.StvarniZavrsetakZadatka,
                   p => p.IdProjekt, p => p.IdVoditelj))
                {
                    ViewBag.Page = page;
                    ViewBag.Sort = sort;
                    ViewBag.Ascending = ascending;
                    try
                    {
                        await ctx.SaveChangesAsync();
                        TempData[RPPP_WebApp.Constants.Message] = "Plan projekta ažuriran.";
                        TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;
                        return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View(planProjekta);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o planu projekta nije moguće povezati s formom");
                    return View(planProjekta);
                }
            }
            catch (Exception exc)
            {
                TempData[RPPP_WebApp.Constants.Message] = exc.CompleteExceptionMessage();
                TempData[RPPP_WebApp.Constants.ErrorOccurred] = true;
                return RedirectToAction(nameof(Edit), id);
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

            var query = ctx.PlanProjekta.AsNoTracking().Where(p => p.IdPlanProjekta == id);

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

            var planoviProjekta = query
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .Include(p => p.IdAktivnosts)
                .ToList();

            var model = new PlanProjektaViewModel
            {
                PlanoviProjekta = planoviProjekta,
                PagingInfo = pagingInfo
            };

            return View(model);
        }

        private async Task PrepareDropDownLists()
        {
            var aktivnost = await ctx.Aktivnosts
                                    .OrderBy(a => a.IdAktivnost)
                                    .Select(a => new { a.IdAktivnost, a.OpisAktivnosti })
                                    .ToListAsync();

            ViewBag.Aktivnost = new SelectList(aktivnost, nameof(Aktivnost.IdAktivnost), nameof(Aktivnost.OpisAktivnosti));

            var projekt = await ctx.Projekts
                                    .OrderBy(a => a.IdProjekt)
                                    .Select(a => new { a.IdProjekt, a.NazivProjekta })
                                    .ToListAsync();

            ViewBag.Projekt = new SelectList(projekt, nameof(Projekt.IdProjekt), nameof(Projekt.NazivProjekta));

            var voditelj = await ctx.VoditeljProjekta
                                    .OrderBy(a => a.IdVoditelj)
                                    .Select(a => new { a.IdVoditelj, a.ImeVoditelja })
                                    .ToListAsync();

            ViewBag.VoditeljProjekta = new SelectList(voditelj, nameof(VoditeljProjektum.IdVoditelj), nameof(VoditeljProjektum.ImeVoditelja));
        }

    }
}
