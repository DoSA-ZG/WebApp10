using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;



namespace RPPP_WebApp.Controllers
{
    public class AktivnostController : Controller
    {
        private readonly ProjektContext ctx;
        private readonly ILogger<AktivnostController> logger;
        private readonly AppSettings appSettings;

        public AktivnostController(ProjektContext ctx, IOptionsSnapshot<AppSettings> options, ILogger<AktivnostController> logger)
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

            var query = ctx.Aktivnosts.AsNoTracking();

            int count = query.Count();
            if (count == 0)
            {
                logger.LogInformation("Ne postoji nijedna aktivnost");
                TempData[RPPP_WebApp.Constants.Message] = "Ne postoji niti jedna aktivnost.";
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

            var aktivnost = query
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .Include(a => a.IdVrstaAktivnostiNavigation)
                .ToList();

            var model = new AktivnostViewModel
            {
                Aktivnost = aktivnost,
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
        public IActionResult Create(AktivnostiViewModel model)
        {
            logger.LogTrace(JsonSerializer.Serialize(model));
            if (ModelState.IsValid)
            {
                try
                {
                    var planProjekta = ctx.PlanProjekta.Include(p => p.IdAktivnosts)
                                                       .FirstOrDefault(p => p.IdPlanProjekta == model.IdPlanProjekta);
                    var aktivnost = new Aktivnost();
                    aktivnost.IdAktivnost = model.IdAktivnost;
                    aktivnost.OpisAktivnosti = model.OpisAktivnosti;
                    aktivnost.DatumPocetka = model.DatumPocetka;
                    aktivnost.DatumZavrsetka = model.DatumZavrsetka;
                    aktivnost.IdVrstaAktivnosti = model.IdVrstaAktivnosti;
                    aktivnost.IdPlanProjekta.Add(planProjekta);

                    ctx.Add(aktivnost);
                    ctx.SaveChanges();
                    logger.LogInformation(new EventId(1000), $"Aktivnost {aktivnost.OpisAktivnosti} dodana.");

                    TempData[RPPP_WebApp.Constants.Message] = $"Aktivnost {aktivnost.OpisAktivnosti} dodana.";
                    TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanja nove aktivnosti: {0}", exc.CompleteExceptionMessage());
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

            try
            {
                var aktivnost = ctx.Aktivnosts
                    .Include(p => p.IdPlanProjekta)
                    .FirstOrDefault(p => p.IdAktivnost == id);

                if (aktivnost != null)
                {
                    foreach (var planProjekta in ctx.PlanProjekta.Include(p => p.IdAktivnosts))
                    {
                        var zahtjevZaBrisanje = planProjekta.IdAktivnosts.FirstOrDefault(z => z.IdAktivnost == id);
                        if (zahtjevZaBrisanje != null)
                        {
                            planProjekta.IdAktivnosts.Remove(zahtjevZaBrisanje);
                            break;
                        }
                    }
                    aktivnost.IdPlanProjekta.Clear();

                    ctx.Remove(aktivnost);
                    ctx.SaveChanges();

                    logger.LogInformation($"Aktivnost {aktivnost.OpisAktivnosti} uspješno obrisana.");
                    TempData[RPPP_WebApp.Constants.Message] = $"Aktivnost {aktivnost.OpisAktivnosti} uspješno obrisana.";
                    TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;
                }
                else
                {
                    logger.LogWarning("Ne postoji aktivnost s ID-om: {0} ", id);
                    TempData[RPPP_WebApp.Constants.Message] = $"Ne postoji aktivnost s ID-om: {id}.";
                    TempData[RPPP_WebApp.Constants.ErrorOccurred] = true;
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception exc)
            {
                logger.LogError("Pogreška prilikom brisanja aktivnosti: {0}", exc.CompleteExceptionMessage());
                TempData[RPPP_WebApp.Constants.Message] = $"Pogreška prilikom brisanja aktivnosti: {exc.CompleteExceptionMessage()}";
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
            var aktivnost = await ctx.Aktivnosts.AsNoTracking().Where(a => a.IdAktivnost == id)
                                                         .Select(a => new AktivnostiViewModel
                                                         {
                                                             IdAktivnost = a.IdAktivnost,
                                                             OpisAktivnosti = a.OpisAktivnosti,
                                                             DatumPocetka = a.DatumPocetka,
                                                             DatumZavrsetka = a.DatumZavrsetka,
                                                             IdVrstaAktivnosti = a.IdVrstaAktivnosti,
                                                             IdPlanProjekta = a.IdPlanProjekta.IsNullOrEmpty() ? 0 : a.IdPlanProjekta.First().IdPlanProjekta
                                                         })
                                                         .SingleOrDefaultAsync();
            if (aktivnost == null)
            {
                logger.LogWarning("Ne postoji aktivnost s ID-om: {0} ", id);
                return NotFound("Ne postoji aktivnost s ID-om: " + id);
            }
            else
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                await PrepareDropDownLists();
                return View(aktivnost);
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
        public async Task<IActionResult> Update(AktivnostiViewModel model, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                Aktivnost aktivnost = await ctx.Aktivnosts
                                  .Where(a => a.IdAktivnost == model.IdAktivnost)
                                  .Include(a => a.IdPlanProjekta)
                                  .FirstOrDefaultAsync();
                PlanProjektum planProjekta = await ctx.PlanProjekta
                                  .Where(a => a.IdPlanProjekta == model.IdPlanProjekta)
                                  .FirstOrDefaultAsync();

                if (aktivnost == null)
                {
                    return NotFound("Neispravan ID aktivnosti: " + model.IdAktivnost);
                }

                aktivnost.IdAktivnost = model.IdAktivnost;
                aktivnost.OpisAktivnosti = model.OpisAktivnosti;
                aktivnost.DatumPocetka = model.DatumPocetka;
                aktivnost.DatumZavrsetka = model.DatumZavrsetka;
                aktivnost.IdVrstaAktivnosti = model.IdVrstaAktivnosti;
                aktivnost.IdPlanProjekta.Clear();
                aktivnost.IdPlanProjekta.Add(planProjekta);
                //planProjekta.IdAktivnosts.Add(aktivnost);

                {
                    ViewBag.Page = page;
                    ViewBag.Sort = sort;
                    ViewBag.Ascending = ascending;
                    try
                    {
                        await ctx.SaveChangesAsync();
                        TempData[RPPP_WebApp.Constants.Message] = "Aktivnost ažurirana.";
                        TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;
                        return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View(aktivnost);
                    }
                }
            }
            catch (Exception exc)
            {
                TempData[RPPP_WebApp.Constants.Message] = exc.CompleteExceptionMessage();
                TempData[RPPP_WebApp.Constants.ErrorOccurred] = true;
                return RedirectToAction(nameof(Edit), model.IdAktivnost);
            }
        }

        /// <summary>
        /// Stvaranje master-detail podataka
        /// </summary>
        /// <param name="aktivnost">Sluzi za stvaranje forme po parametrima objekta</param>
        /// <param name="idPlanProjekta">Sluzi za stvaranje forme po parametrima objekta</param>
        /// <returns>Vraca master-detail prikaz s novim podatkom</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create2(Aktivnost aktivnost, int idPlanProjekta)
        {
            logger.LogTrace(JsonSerializer.Serialize(aktivnost));

            if (ModelState.IsValid)
            {
                try
                {
                    var planProjekta = ctx.PlanProjekta.Find(idPlanProjekta);
                    aktivnost.IdPlanProjekta.Add(planProjekta);
                    ctx.Add(aktivnost);
                    ctx.SaveChanges();
                    logger.LogInformation(new EventId(1000), $"Aktivnost {aktivnost.OpisAktivnosti} dodana.");

                    TempData[RPPP_WebApp.Constants.Message] = "Aktivnost dodana.";
                    TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;

                    return RedirectToAction("Edit", "PlanProjekta", new { id = idPlanProjekta });
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanja nove aktivnosti: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());

                    return View(aktivnost);
                }
            }
            else
            {
                return View(aktivnost);
            }
        }

        /// <summary>
        /// Spremanje master-detail podatka nakon promjena
        /// </summary>
        /// <param name="idPlanProjekta">Id plana projekta koji se ureduje</param>
        /// <param name="id">Id aktivnosti</param>
        /// <param name="page">Stranica na kojoj se nalazimo</param>
        /// <param name="sort">Vrijednost po kojoj se sortira</param>
        /// <param name="ascending">Redoslijed sortiranja</param>
        /// <returns>Vraca master-detail podatak</returns>
        [HttpPost, ActionName("Edit2")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update2(int idPlanProjekta, int id, int page, int sort, bool ascending = true)
        {
            try
            {
                Aktivnost aktivnost = await ctx.Aktivnosts
                    .Where(t => t.IdAktivnost == id).FirstOrDefaultAsync();

                if (aktivnost == null)
                {
                    return NotFound("Neispravan ID aktivnosti: " + id);
                }

                if (await TryUpdateModelAsync<Aktivnost>(aktivnost, "",
                     t => t.IdAktivnost, t => t.OpisAktivnosti, t => t.DatumPocetka, t => t.DatumZavrsetka, t => t.IdVrstaAktivnosti))
                {
                    ViewBag.Page = page;
                    ViewBag.Sort = sort;
                    ViewBag.Ascending = ascending;

                    try
                    {
                        await ctx.SaveChangesAsync();

                        TempData[RPPP_WebApp.Constants.Message] = "Aktivnost ažurirana.";
                        TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;

                        return RedirectToAction("Edit", "PlanProjekta", new { id = idPlanProjekta, page = page, sort = sort, ascending = ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View(aktivnost);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o aktivnosti nije moguće povezati s formom");
                    return View(aktivnost);
                }
            }
            catch (Exception exc)
            {
                TempData[RPPP_WebApp.Constants.Message] = exc.CompleteExceptionMessage();
                TempData[RPPP_WebApp.Constants.ErrorOccurred] = true;

                return RedirectToAction("Edit", "PlanProjekta", new { id = idPlanProjekta, page = page, sort = sort, ascending = ascending });
            }
        }

        /// <summary>
        /// Brisanje master-detail podataka
        /// </summary>
        /// <param name="idPlanProjekta">Id plana projekta</param>
        /// <param name="id">Id aktivnosti</param>
        /// <returns>Vraca poslijednju aktivnu stranicu</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete2(int idPlanProjekta, int id)
        {
            var aktivnost = ctx.Aktivnosts.Find(id);

            if (aktivnost != null)
            {
                try
                {
                    foreach (var planProjekta in ctx.PlanProjekta.Include(p => p.IdAktivnosts))
                    {
                        var zahtjevZaBrisanje = planProjekta.IdAktivnosts.FirstOrDefault(z => z.IdAktivnost == id);
                        if (zahtjevZaBrisanje != null)
                        {
                            planProjekta.IdAktivnosts.Remove(zahtjevZaBrisanje);
                            break;
                        }
                    }
                    ctx.Remove(aktivnost);
                    ctx.SaveChanges();

                    logger.LogInformation($"Aktivnost uspješno obrisana");
                    TempData[RPPP_WebApp.Constants.Message] = $"Aktivnost uspješno obrisana";
                    TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[RPPP_WebApp.Constants.Message] = "Pogreška prilikom brisanja aktivnosti: " + exc.CompleteExceptionMessage();
                    TempData[RPPP_WebApp.Constants.ErrorOccurred] = true;
                    logger.LogError("Pogreška prilikom brisanja aktivnosti: " + exc.CompleteExceptionMessage());
                }
            }
            else
            {
                logger.LogWarning("Ne postoji aktivnost s ID-om: {0} ", id);
                TempData[RPPP_WebApp.Constants.Message] = "Ne postoji aktivnost s ID-om: " + id;
                TempData[RPPP_WebApp.Constants.ErrorOccurred] = true;
            }

            return RedirectToAction("Edit", "PlanProjekta", new { id = idPlanProjekta, });
        }

        /// <summary>
        /// Stvaranje dropdown liste
        /// </summary>
        /// <returns>Vraca podatke u obliku dropdown liste</returns>
        private async Task PrepareDropDownLists()
        {
            var vrstaAktivnosti = await ctx.VrstaAktivnostis
                                    .OrderBy(a => a.IdVrstaAktivnosti)
                                    .Select(a => new { a.IdVrstaAktivnosti, a.NazivAktivnosti })
                                    .ToListAsync();

            ViewBag.VrstaAktivnosti = new SelectList(vrstaAktivnosti, nameof(VrstaAktivnosti.IdVrstaAktivnosti), nameof(VrstaAktivnosti.NazivAktivnosti));

            var planProjekta = await ctx.PlanProjekta
                                .OrderBy(p => p.IdPlanProjekta)
                                .Select(p => new { p.IdPlanProjekta })
                                .ToListAsync();

            ViewBag.PlanProjekta = new SelectList(planProjekta, nameof(PlanProjektum.IdPlanProjekta), nameof(PlanProjektum.IdPlanProjekta));
        }
    }

}
