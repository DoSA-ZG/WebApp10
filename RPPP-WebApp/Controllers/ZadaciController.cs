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
 

        public class ZadatakController : Controller
        {
            private readonly ProjektContext ctx;
            private readonly ILogger<ZadatakController> logger;
            private readonly AppSettings appSettings;

            public ZadatakController(ProjektContext ctx, IOptionsSnapshot<AppSettings> options, ILogger<ZadatakController> logger)
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

                var query = ctx.Zadacis.AsNoTracking();

                int count = query.Count();
                if (count == 0)
                {
                    logger.LogInformation("Ne postoji nijedan zadatak");
                    TempData[RPPP_WebApp.Constants.Message] = "Ne postoji niti jedan zadatak.";
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

                var zadaci = query
                    .Skip((page - 1) * pagesize)
                    .Take(pagesize)
                    .Include(z => z.IdZahtjevNavigation)
                    .Include(z => z.IdPrioritetZadatkaNavigation)
                    .ToList();

                var model = new ZadaciViewModel
                {
                    Zadaci = zadaci,
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
        /// <param name="zadatak">Sluzi za stvaranje parametara forme</param>
        /// <returns>Vraca rezultat stvaranja novog podatka</returns>
        [HttpPost]
            [ValidateAntiForgeryToken]
            public IActionResult Create(Zadaci zadatak)
            {
                logger.LogTrace(JsonSerializer.Serialize(zadatak));
                if (ModelState.IsValid)
                {
                    try
                    {
                        ctx.Add(zadatak);
                        ctx.SaveChanges();
                        logger.LogInformation(new EventId(1000), $"Zadatak {zadatak.Opis} dodan.");

                        TempData[RPPP_WebApp.Constants.Message] = $"Zadatak {zadatak.Opis} dodan.";
                        TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception exc)
                    {
                        logger.LogError("Pogreška prilikom dodavanje novog zadatka: {0}", exc.CompleteExceptionMessage());
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View(zadatak);
                    }
                }
                else
                {
                    return View(zadatak);
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
            public IActionResult Delete(int id , int page , int sort , bool ascending = true)
            {
           
            var zadatak = ctx.Zadacis.Find(id);

                if (zadatak != null)
                {
                    try
                    {
                        ctx.Remove(zadatak);
                        ctx.SaveChanges();
                        logger.LogInformation($"Zadatak  uspješno obrisan");
                        TempData[RPPP_WebApp.Constants.Message] = $"Zadatak  uspješno obrisan";
                        TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;
                    }
                    catch (Exception exc)
                    {
                        TempData[RPPP_WebApp.Constants.Message] = "Pogreška prilikom brisanja zadatka: " + exc.CompleteExceptionMessage();
                        TempData[RPPP_WebApp.Constants.ErrorOccurred] = true;
                        logger.LogError("Pogreška prilikom brisanja zadatka: " + exc.CompleteExceptionMessage());
                    }
                }
                else
                {
                    logger.LogWarning("Ne postoji zadatak s ID-om: {0} ", id);
                    TempData[RPPP_WebApp.Constants.Message] = "Ne postoji zadatak s ID-om: " + id;
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
            public async Task<IActionResult> Edit(int id, int page , int sort , bool ascending = true)
            {
                var zadatak = ctx.Zadacis.AsNoTracking().Where(z => z.IdZadatak == id).SingleOrDefault();
                if (zadatak == null)
                {
                    logger.LogWarning("Ne postoji zadatak s ID-om: {0} ", id);
                    return NotFound("Ne postoji zadatak s ID-om: " + id);
                }
                else
                {
                    ViewBag.Page = page;
                    ViewBag.Sort = sort;
                    ViewBag.Ascending = ascending;
                await PrepareDropDownLists();
                return View(zadatak);
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
            public async Task<IActionResult> Update(int id, int page , int sort , bool ascending = true)
            {
                try
                {
                    Zadaci zadatak = await ctx.Zadacis
                                      .Where(z => z.IdZadatak == id)
                                      .FirstOrDefaultAsync();
                    if (zadatak == null)
                    {
                        return NotFound("Neispravan ID zadatka: " + id);
                    }

                    if (await TryUpdateModelAsync<Zadaci>(zadatak, "",
                     z => z.Status, z => z.Aktivan, z => z.Opis, z => z.IdZahtjev, z => z.NositeljZadatka, z => z.IdPrioritetZadatka
                 ))
                    {
                        ViewBag.Page = page;
                        ViewBag.Sort = sort;
                        ViewBag.Ascending = ascending;
                        try
                        {
                            await ctx.SaveChangesAsync();
                            TempData[RPPP_WebApp.Constants.Message] = "Zadatak ažuriran.";
                            TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;
                            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
                        }
                        catch (Exception exc)
                        {
                            ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                            return View(zadatak);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Podatke o zadatku nije moguće povezati s formom");
                        return View(zadatak);
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
        /// Brisanje master-detail podataka
        /// </summary>
        /// <param name="id">Id zadatka</param>
        /// <param name="page">Stranica na kojoj se nalazimo</param>
        /// <param name="sort">Vrijednost po kojoj se sortira</param>
        /// <param name="ascending">Redoslijed sortiranja</param>
        /// <returns>Vraca poslijednju aktivnu stranicu</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete2(int id, int page, int sort, bool ascending = true)
        {
            var zadatak = ctx.Zadacis.Find(id);

            if (zadatak != null)
            {
                try
                {
                    ctx.Remove(zadatak);
                    ctx.SaveChanges();
                    logger.LogInformation($"Zadatak uspješno obrisan");
                    TempData[RPPP_WebApp.Constants.Message] = $"Zadatak uspješno obrisan";
                    TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[RPPP_WebApp.Constants.Message] = "Pogreška prilikom brisanja zadatka: " + exc.CompleteExceptionMessage();
                    TempData[RPPP_WebApp.Constants.ErrorOccurred] = true;
                    logger.LogError("Pogreška prilikom brisanja zadatka: " + exc.CompleteExceptionMessage());
                }
            }
            else
            {
                logger.LogWarning("Ne postoji zadatak s ID-om: {0} ", id);
                TempData[RPPP_WebApp.Constants.Message] = "Ne postoji zadatak s ID-om: " + id;
                TempData[RPPP_WebApp.Constants.ErrorOccurred] = true;
            }

          
            return RedirectToAction("Edit", "Zahtjev", new { id = zadatak.IdZahtjev, page = page, sort = sort, ascending = ascending });
        }

        /// <summary>
        /// Prikaz forme za uredivanje
        /// </summary>
        /// <param name="id">Id podatka koji se ureduje</param>
        /// <param name="page">Stranica na kojoj se nalazimo</param>
        /// <param name="sort">Vrijednost po kojoj se sortira</param>
        /// <param name="ascending">Redoslijed sortiranja</param>
        /// <returns>Vraca master-detail podatak</returns>
        [HttpGet]
        public async Task<IActionResult> Edit2(int id, int page, int sort, bool ascending = true)
        {
            var zadatak = ctx.Zadacis.AsNoTracking().Where(z => z.IdZadatak == id).SingleOrDefault();
            if (zadatak == null)
            {
                logger.LogWarning("Ne postoji zadatak s ID-om: {0} ", id);
                return NotFound("Ne postoji zadatak s ID-om: " + id);
            }
            else
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                await PrepareDropDownLists();
                return View(zadatak);
            }
        }
        /// <summary>
        /// Spremanje master-detail podatka nakon promjena
        /// </summary>
        /// <param name="id">Id zadatka</param>
        /// <param name="page">Stranica na kojoj se nalazimo</param>
        /// <param name="sort">Vrijednost po kojoj se sortira</param>
        /// <param name="ascending">Redoslijed sortiranja</param>
        /// <returns>Vraca master-detail podatak</returns>
        [HttpPost, ActionName("Edit2")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update2(int id, int page, int sort, bool ascending = true)
        {
            try
            {
                Zadaci zadatak = await ctx.Zadacis
                                     .Where(z => z.IdZadatak == id)
                                     .FirstOrDefaultAsync();
                if (zadatak == null)
                {
                    return NotFound("Neispravan ID zadatka: " + id);
                }

                if (await TryUpdateModelAsync<Zadaci>(zadatak, "",
                    z => z.Status, z => z.Aktivan, z => z.Opis, z => z.IdZahtjev, z => z.NositeljZadatka, z => z.IdPrioritetZadatka))
                {
                    ViewBag.Page = page;
                    ViewBag.Sort = sort;
                    ViewBag.Ascending = ascending;
                    try
                    {
                        await ctx.SaveChangesAsync();
                        TempData[RPPP_WebApp.Constants.Message] = "Zadatak ažuriran.";
                        TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;

                        
                        return RedirectToAction("Edit", "Zahtjev", new { id = zadatak.IdZahtjev, page = page, sort = sort, ascending = ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View(zadatak);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o zadatku nije moguće povezati s formom");
                    return View(zadatak);
                }
            }
            catch (Exception exc)
            {
                TempData[RPPP_WebApp.Constants.Message] = exc.CompleteExceptionMessage();
                TempData[RPPP_WebApp.Constants.ErrorOccurred] = true;

                
                return RedirectToAction("Edit", "Zahtjev", new { id = id, page = page, sort = sort, ascending = ascending });
            }
        }
        /// <summary>
        /// Prikaz forme stvaranja
        /// </summary>
        /// <returns>Vraca prikaz forme</returns>
        [HttpGet]
        public async Task<IActionResult> Create2()
        {
            await PrepareDropDownLists();
            return View();
        }
        /// <summary>
        /// Stvaranje master-detail podataka
        /// </summary>
        /// <param name="zadatak">Sluzi za stvaranje forme po parametrima objekta</param>
        /// <returns>Vraca master-detail prikaz s novim podatkom</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create2(Zadaci zadatak)
        {
            logger.LogTrace(JsonSerializer.Serialize(zadatak));

            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(zadatak);
                    ctx.SaveChanges();
                    logger.LogInformation(new EventId(1000), $"Zadatak {zadatak.Opis} dodan.");

                    TempData[RPPP_WebApp.Constants.Message] = $"Zadatak {zadatak.Opis} dodan.";
                    TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;

                  
                    int idZahtjev = zadatak.IdZahtjev;

                   
                    return RedirectToAction("Edit", "Zahtjev", new { id = idZahtjev });
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanje novog zadatka: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());

                    return View(zadatak);
                }
            }
            else
            {
                return View(zadatak);
            }
        }
        private async Task PrepareDropDownLists()
        {
            
            var zahtjevi = await ctx.Zahtjevs
                                .OrderBy(z => z.IdZahtjev)
                                .Select(z => new { z.IdZahtjev, z.Opis })
                                .ToListAsync();

            ViewBag.Zahtjevi = new SelectList(zahtjevi, nameof(Zahtjev.IdZahtjev), nameof(Zahtjev.Opis));

            
            var prioriteti = await ctx.PrioritetZadatkas
                                    .OrderBy(p => p.IdPrioritetZadatka)
                                    .Select(p => new { p.IdPrioritetZadatka, p.RazinaPrioriteta })
                                    .ToListAsync();

            ViewBag.Prioriteti = new SelectList(prioriteti, nameof(PrioritetZadatka.IdPrioritetZadatka), nameof(PrioritetZadatka.RazinaPrioriteta));
        }

    }

}
