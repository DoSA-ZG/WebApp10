using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using System.Text.Json;
using RPPP_WebApp.Extensions.Selectors;


namespace RPPP_WebApp.Controllers
{


    public class ProjektiController : Controller
    {
        private readonly ProjektContext ctx;
        private readonly ILogger<ProjektiController> logger;
        private readonly AppSettings appSettings;

        public ProjektiController(ProjektContext ctx, IOptionsSnapshot<AppSettings> options, ILogger<ProjektiController> logger)
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

            var query = ctx.Projekts.AsNoTracking();

            int count = query.Count();
            if (count == 0)
            {
                logger.LogInformation("Ne postoji nijedan projekt");
                TempData[RPPP_WebApp.Constants.Message] = "Ne postoji niti jedan projekt.";
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

            var projekti = query
                .Skip((page - 1) * pagesize)
                .Take(pagesize).Include(p => p.IdVrstaProjektaNavigation)
                .Include(p => p.ProjektnaDokumentacijas)
                .ToList();

            var model = new ProjektiViewModel
            {
                projekti = projekti,
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

        private async Task PrepareDropDownLists()
        {
            var vrste = await ctx.VrstaProjekta
                        .OrderBy(v => v.IdVrstaProjekta)
                        .Select(v => new { v.IdVrstaProjekta, v.NazivVrsteProjekta })
                        .ToListAsync();
            ViewBag.Vrste = new SelectList(vrste, nameof(VrstaProjektum.IdVrstaProjekta), nameof(VrstaProjektum.NazivVrsteProjekta));
            var vrsteDokumenata = await ctx.VrstaDokumenta
                               .OrderBy(v => v.IdVrstaDokument)
                               .Select(v => new { v.IdVrstaDokument, v.NazivVrste })
                               .ToListAsync();
            ViewBag.Dokumenti = new SelectList(vrsteDokumenata, nameof(VrstaDokumentum.IdVrstaDokument), nameof(VrstaDokumentum.NazivVrste));
            var projekti = await ctx.Projekts
                               .OrderBy(p => p.IdProjekt)
                               .Select(p => new { p.IdProjekt, p.NazivProjekta })
                               .ToListAsync();
            ViewBag.Projekti = new SelectList(projekti, nameof(Projekt.IdProjekt), nameof(Projekt.NazivProjekta));
        }
        /// <summary>
        /// Stvaranje novog podatka
        /// </summary>
        /// <param name="projekt">Sluzi za stvaranje parametara forme</param>
        /// <returns>Vraca rezultat stvaranja novog podatka</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Projekt projekt)
        {
            logger.LogTrace(JsonSerializer.Serialize(projekt));
            if (ModelState.IsValid)
            {
                try
                {
                    if(projekt.IdProjekt < 0)
                    {
                        Console.WriteLine("IdProjekta mora biti pozitivan!");
                        ModelState.AddModelError(string.Empty, "IdProjekta mora biti pozitivan!");
                        return RedirectToAction("Create", "Projekti", projekt);
                    }
                    if (null != ctx.Projekts.Find(projekt.IdProjekt))
                    {
                        Console.WriteLine($"Projekt s idjem: {projekt.IdProjekt} veæ postoji!");
                        ModelState.AddModelError(string.Empty, $"Projekt s idjem: {projekt.IdProjekt} veæ postoji!");
                        return RedirectToAction("Create", "Projekti", projekt);
                    }
                    if (null == ctx.VrstaProjekta.Find(projekt.IdVrstaProjekta))
                    {
                        Console.WriteLine($"Neispravan id vrste projekta!");
                        ModelState.AddModelError(string.Empty, $"Neispravan id vrste projekta!");
                        return RedirectToAction("Create", "Projekti", projekt);
                    }
                    ctx.Add(projekt);
                    ctx.SaveChanges();
                    logger.LogInformation(new EventId(1000), $"Projekt {projekt.NazivProjekta} dodan.");

                    TempData[RPPP_WebApp.Constants.Message] = $"Projekt {projekt.NazivProjekta} dodan.";
                    TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanje novog projekta: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(projekt);
                }
            }
            else
            {
                return View(projekt);
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

            var projekt = ctx.Projekts.Find(id);

            if (projekt != null)
            {
                try
                {
                    ctx.Remove(projekt);
                    ctx.SaveChanges();
                    logger.LogInformation($"Projekt uspješno obrisan");
                    TempData[RPPP_WebApp.Constants.Message] = $"Projekt  uspješno obrisan";
                    TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[RPPP_WebApp.Constants.Message] = "Pogreška prilikom brisanja projekta: " + exc.CompleteExceptionMessage();
                    TempData[RPPP_WebApp.Constants.ErrorOccurred] = true;
                    logger.LogError("Pogreška prilikom brisanja projekta: " + exc.CompleteExceptionMessage());
                }
            }
            else
            {
                logger.LogWarning("Ne postoji projekt s ID-om: {0} ", id);
                TempData[RPPP_WebApp.Constants.Message] = "Ne postoji projekt s ID-om: " + id;
                TempData[RPPP_WebApp.Constants.ErrorOccurred] = true;
            }
            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
        }

        /// <summary>
        /// Uredivanje master detail podatka
        /// </summary>
        /// <param name="id">Id podatka koji se ureduje</param>
        /// <param name="page">Stranica na kojoj se nalazimo</param>
        /// <param name="sort">Vrijednost po kojoj se sortira</param>
        /// <param name="ascending">Redoslijed sortiranja</param>
        /// <returns>Vraca master-detail podatak</returns>
        [HttpGet]
        public async Task<IActionResult> Edit2(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            await PrepareDropDownLists();
            var projekt = ctx.Projekts.AsNoTracking().Include(p => p.IdVrstaProjektaNavigation).Include(p => p.ProjektnaDokumentacijas).Where(pd => pd.IdProjekt == id).SingleOrDefault();
            if (projekt == null)
            {
                logger.LogWarning("Ne postoji projekt s ID-om: {0} ", id);
                return NotFound("Ne postoji projekt s ID-om: " + id);
            }
            else
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                return View(projekt);
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
            await PrepareDropDownLists();
            var projekt = await ctx.Projekts
                                .Where(d => d.IdProjekt == id).Include(p => p.IdVrstaProjektaNavigation).Include(p => p.ProjektnaDokumentacijas)
                                .Select(d => new ProjektViewModel
                                {
                                    IdProjekt = d.IdProjekt,
                                    KraticaProjekta = d.KraticaProjekta,
                                    NazivProjekta = d.NazivProjekta,
                                    DatumPocetka = d.DatumPocetka,
                                    DatumZavrsetka = d.DatumZavrsetka,
                                    IdVrstaProjekta = d.IdVrstaProjekta,
                                })
                                .FirstOrDefaultAsync();
            if (projekt == null)
            {
                return NotFound($"Projekt {id} ne postoji");
            }
            else
            {
                //uèitavanje stavki
                var dokumenti = await ctx.ProjektnaDokumentacijas
                                      .Where(s => s.IdProjekt == projekt.IdProjekt)
                                      .OrderBy(s => s.IdProjekt)
                                      .Select(d => new ProjektnaDokumentacija
                                      {
                                          IdDokument = d.IdDokument,
                                          NazivDokumenta = d.NazivDokumenta,
                                          FormatDokumenta = d.FormatDokumenta,
                                          IdVrstaDokument = d.IdVrstaDokument
                                      })
                                      .ToListAsync();
                projekt.ProjektnaDokumentacijas = dokumenti;

                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;

                return View(projekt);

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
        public async Task<IActionResult> Update(ProjektViewModel model, int page, int sort, bool ascending = true)
        {
            Console.WriteLine(model.IdProjekt);
            ViewBag.Page = page;
            ViewBag.Sort = sort;
            ViewBag.Ascending = ascending;
            if (ModelState.IsValid)
            {
                var projekt = await ctx.Projekts
                                        .Include(d => d.ProjektnaDokumentacijas)
                                        .Where(d => d.IdProjekt == model.IdProjekt)
                                        .FirstOrDefaultAsync();
                if (projekt == null)
                {
                    return NotFound("Ne postoji projekt s id-om: " + model.IdProjekt);
                }

                projekt.IdProjekt = model.IdProjekt;
                projekt.KraticaProjekta = model.KraticaProjekta;
                projekt.IdVrstaProjekta = model.IdVrstaProjekta;
                projekt.NazivProjekta = model.NazivProjekta;
                projekt.DatumPocetka = model.DatumPocetka;
                projekt.DatumZavrsetka = model.DatumZavrsetka;
                try
                {

                    await ctx.SaveChangesAsync();

                    TempData[Constants.Message] = $"Zahtjev {projekt.IdProjekt} uspješno ažuriran.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Edit), new
                    {
                        id = projekt.IdProjekt,
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
        /*
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update2(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                Projekt projekt = await ctx.Projekts
                                  .Where(pd => pd.IdProjekt == id)
                                  .FirstOrDefaultAsync();
                if (projekt == null)
                {
                    return NotFound("Neispravan ID projekta: " + id);
                }

                if (await TryUpdateModelAsync<Projekt>(projekt, "",
                 p => p.KraticaProjekta, p => p.NazivProjekta, p => p.DatumPocetka, p => p.DatumZavrsetka, p => p.IdVrstaProjekta))
                {
                    ViewBag.Page = page;
                    ViewBag.Sort = sort;
                    ViewBag.Ascending = ascending;
                    try
                    {
                        await ctx.SaveChangesAsync();
                        TempData[RPPP_WebApp.Constants.Message] = "Projekt ažuriran.";
                        TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;
                        return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View(projekt);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o projektu nije moguæe povezati s formom");
                    return View(projekt);
                }
            }
            catch (Exception exc)
            {
                TempData[RPPP_WebApp.Constants.Message] = exc.CompleteExceptionMessage();
                TempData[RPPP_WebApp.Constants.ErrorOccurred] = true;
                return RedirectToAction(nameof(Edit), id);
            }
        }
        */
        /// <summary>
        /// Master-detail prikaz
        /// </summary>
        /// <param name="id">Id podatka koji se prikazuje</param>
        /// <param name="page">Stranica na kojoj se nalazimo</param>
        /// <param name="sort">Vrijednost po kojoj se sortira</param>
        /// <param name="ascending">Redoslijed sortiranja</param>
        /// <returns>Vraca master-detail prikaz podataka</returns>
        public async Task<IActionResult> STP(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            await PrepareDropDownLists();
            int pageSize = appSettings.PageSize;


            var query = ctx.Projekts.AsNoTracking().Where(p => p.IdProjekt == id);


            int count = query.Count();


            var pagingInfo = new PagingInfo

            {
                CurrentPage = page,
                Sort = sort,
                Ascending = ascending,

                ItemsPerPage = pageSize,
                TotalItems = count
            };

            if (page < 1 || page > pagingInfo.TotalPages)

            {

                return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });

            }


            var projekti = query

                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(p => p.ProjektnaDokumentacijas)
                .Include(p => p.IdVrstaProjektaNavigation)
                .ToList();


            var model = new ProjektiViewModel

            {

                projekti = projekti,

                PagingInfo = pagingInfo

            };
            return View(model);

        }
    }

}
