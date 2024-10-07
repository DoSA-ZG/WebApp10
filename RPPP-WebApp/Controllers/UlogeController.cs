using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;

using System.Text.Json;

namespace RPPP_WebApp.Controllers
{


    public class UlogaController : Controller
    {
        private readonly ProjektContext ctx;
        private readonly ILogger<UlogaController> logger;
        private readonly AppSettings appSettings;

        public UlogaController(ProjektContext ctx, IOptionsSnapshot<AppSettings> options, ILogger<UlogaController> logger)
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

            var query = ctx.Uloges.AsNoTracking();

            int count = query.Count();
            if (count == 0)
            {
                logger.LogInformation("Ne postoji nijedna uloga");
                TempData[Constants.Message] = "Ne postoji niti jedna uloga.";
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

            var uloge = query
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .Include(u => u.IdVrstaUlogaNavigation)
                .ToList();

            var model = new UlogeViewModel
            {
                Uloge = uloge,
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
        /// <param name="uloga">Sluzi za stvaranje parametara forme</param>
        /// <returns>Vraca rezultat stvaranja novog podatka</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Uloge uloga)
        {
            logger.LogTrace(JsonSerializer.Serialize(uloga));
            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(uloga);
                    ctx.SaveChanges();
                    logger.LogInformation(new EventId(1000), $"Uloga {uloga.OpisUloga} dodana.");

                    TempData[Constants.Message] = $"Uloga {uloga.OpisUloga} dodana.";
                    TempData[Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanja nove uloge: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                    return View(uloga);
                }
            }
            else
            {
                return View(uloga);
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

            var uloga = ctx.Uloges.Find(id);

            if (uloga != null)
            {
                try
                {
                    var sudjelujuRecords = ctx.Sudjelujus.Where(s => s.IdUloga == id);
                    ctx.Sudjelujus.RemoveRange(sudjelujuRecords);

                    ctx.Remove(uloga);
                    ctx.SaveChanges();
                    logger.LogInformation($"Uloga  uspješno obrisana");
                    TempData[Constants.Message] = $"Uloga  uspješno obrisana";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = "Pogreška prilikom brisanja uloge: " + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                    logger.LogError("Pogreška prilikom brisanja uloge: " + exc.CompleteExceptionMessage());
                }
            }
            else
            {
                logger.LogWarning("Ne postoji uloga s ID-om: {0} ", id);
                TempData[Constants.Message] = "Ne postoji uloga s ID-om: " + id;
                TempData[Constants.ErrorOccurred] = true;
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
        public async Task<IActionResult> Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            var uloga = ctx.Uloges.AsNoTracking().Where(u => u.IdUloga == id).SingleOrDefault();
            if (uloga == null)
            {
                logger.LogWarning("Ne postoji uloga s ID-om: {0} ", id);
                return NotFound("Ne postoji uloga s ID-om: " + id);
            }
            else
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                await PrepareDropDownLists();
                return View(uloga);
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
        public async Task<IActionResult> Update(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            try
            {
                Uloge uloga = await ctx.Uloges
                                  .Where(u => u.IdUloga == id)
                                  .FirstOrDefaultAsync();
                if (uloga == null)
                {
                    return NotFound("Neispravan ID uloge: " + id);
                }

                if (await TryUpdateModelAsync<Uloge>(uloga, "",
                 u => u.IdVrstaUloga, u => u.OpisUloga
             ))
                {
                    ViewBag.Page = page;
                    ViewBag.Sort = sort;
                    ViewBag.Ascending = ascending;
                    try
                    {
                        await ctx.SaveChangesAsync();
                        TempData[Constants.Message] = "Uloga ažurirana.";
                        TempData[Constants.ErrorOccurred] = false;
                        return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View(uloga);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o ulozi nije moguće povezati s formom");
                    return View(uloga);
                }
            }
            catch (Exception exc)
            {
                TempData[Constants.Message] = exc.CompleteExceptionMessage();
                TempData[Constants.ErrorOccurred] = true;
                return RedirectToAction(nameof(Edit), id);
            }
        }
        /// <summary>
        /// Stvaranje master-detail podataka
        /// </summary>
        /// <param name="IdProjekt">Id projekta za koji se stvara master-detail</param>
        /// <param name="uloga">Uloga s kojom se stvara master-detail</param>
        /// <param name="sudionik">Id sudionika</param>
        /// <returns>Vraca master-detail prikaz s novim podatkom</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create2(int IdProjekt, Uloge uloga, int sudionik)
        {
            logger.LogTrace(JsonSerializer.Serialize(uloga));

            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(uloga);

                    var sudjeluju = new Sudjeluju
                    {
                        IdProjekt = IdProjekt,
                        IdUloga = uloga.IdUloga,
                        IdSudionik = sudionik
                    };
                    ctx.Sudjelujus.Add(sudjeluju);
                    ctx.SaveChanges();

                    TempData[Constants.Message] = $"Uloga {uloga.OpisUloga} dodan.";
                    TempData[Constants.ErrorOccurred] = false;
                    logger.LogInformation(new EventId(1000), $"Uloga {uloga.OpisUloga} dodan.");

                    int idSudionik = sudionik;


                    return RedirectToAction("Edit", "Sudionik", new { id = idSudionik });
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanje novog sudionika: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());

                    return View(uloga);
                }
            }
            else
            {
                return View(uloga);
            }
        }
        /// <summary>
        /// Brisanje master-detail podataka
        /// </summary>
        /// <param name="sudionik">Id sudionika</param>
        /// <param name="id">Id uloge</param>
        /// <returns>Vraca poslijednju aktivnu stranicu</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete2(int sudionik, int id)
        {
            var uloga = ctx.Uloges.Find(id);

            if (uloga != null)
            {
                try
                {

                    var sudjelujuRecords = ctx.Sudjelujus.Where(s => s.IdUloga == id);
                    ctx.Sudjelujus.RemoveRange(sudjelujuRecords);

                    ctx.Remove(uloga);
                    ctx.SaveChanges();

                    logger.LogInformation($"Uloga uspješno obrisana");
                    TempData[Constants.Message] = $"Uloga uspješno obrisana";
                    TempData[Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[Constants.Message] = "Pogreška prilikom brisanja uloge: " + exc.CompleteExceptionMessage();
                    TempData[Constants.ErrorOccurred] = true;
                    logger.LogError("Pogreška prilikom brisanja uloge: " + exc.CompleteExceptionMessage());
                }
            }
            else
            {
                logger.LogWarning("Ne postoji uloga s ID-om: {0} ", id);
                TempData[Constants.Message] = "Ne postoji uloga s ID-om: " + id;
                TempData[Constants.ErrorOccurred] = true;
            }

            return RedirectToAction("Edit", "Sudionik", new { id = sudionik });
        }
        /// <summary>
        /// Spremanje master-detail podatka nakon promjena
        /// </summary>
        /// <param name="sudionik">Id sudionika koji se ureduje</param>
        /// <param name="id">Id aktivnosti</param>
        /// <param name="page">Stranica na kojoj se nalazimo</param>
        /// <param name="sort">Vrijednost po kojoj se sortira</param>
        /// <param name="ascending">Redoslijed sortiranja</param>
        /// <returns>Vraca master-detail podatak</returns>
        [HttpPost, ActionName("Edit2")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update2(int sudionik, int id, int page, int sort, bool ascending = true)
        {
            try
            {
                Uloge uloga = await ctx.Uloges
                                     .Where(z => z.IdUloga == id)
                                     .FirstOrDefaultAsync();
                if (uloga == null)
                {
                    return NotFound("Neispravan ID uloge: " + id);
                }

                if (await TryUpdateModelAsync<Uloge>(uloga, "",
                    z => z.IdUloga, z => z.IdVrstaUloga, z => z.OpisUloga))
                {
                    ViewBag.Page = page;
                    ViewBag.Sort = sort;
                    ViewBag.Ascending = ascending;
                    try
                    {
                        await ctx.SaveChangesAsync();
                        TempData[Constants.Message] = "Uloga ažurirana.";
                        TempData[Constants.ErrorOccurred] = false;


                        return RedirectToAction("Edit", "Sudionik", new { id = sudionik, page = page, sort = sort, ascending = ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View(uloga);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke za ulogu nije moguće povezati s formom");
                    return View(uloga);
                }
            }
            catch (Exception exc)
            {
                TempData[Constants.Message] = exc.CompleteExceptionMessage();
                TempData[Constants.ErrorOccurred] = true;


                return RedirectToAction("Edit", "Sudionik", new { id = sudionik, page = page, sort = sort, ascending = ascending });
            }
        }
        private async Task PrepareDropDownLists()
        {
            var vrstaUloge = await ctx.VrstaUloges
                                .OrderBy(v => v.IdVrstaUloga)
                                .Select(v => new { v.IdVrstaUloga, v.Naziv })
                                .ToListAsync();

            ViewBag.VrsteUloga = new SelectList(vrstaUloge, nameof(VrstaUloge.IdVrstaUloga), nameof(VrstaUloge.Naziv));
        }
    }
}
