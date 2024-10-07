using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Threading.Tasks;
namespace RPPP_WebApp.Controllers

{

    public class TransakcijeController : Controller

    {

        private readonly ProjektContext ctx;

        private readonly ILogger<TransakcijeController> logger;

        private readonly AppSettings appSettings;

        public TransakcijeController(ProjektContext ctx, IOptionsSnapshot<AppSettings> options, ILogger<TransakcijeController> logger)

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

            var query = ctx.Transakcijas.AsNoTracking();

            int count = query.Count();

            if (count == 0)

            {

                logger.LogInformation("Ne postoji nijedna transakcija");

                TempData[RPPP_WebApp.Constants.Message] = "Ne postoji niti jedna transakcija.";

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

            var transakcije = query

                .Skip((page - 1) * pagesize)

                .Take(pagesize)
                 .Include(z => z.IdVrstaTransakcijeNavigation)
                .ToList();

            var model = new TransakcijeViewModel

            {

                Transakcije = transakcije,

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
        /// <param name="transakcija">Sluzi za stvaranje parametara forme</param>
        /// <returns>Vraca rezultat stvaranja novog podatka</returns>
        [HttpPost]

        [ValidateAntiForgeryToken]

        public IActionResult Create(Transakcija transakcija)

        {

            logger.LogTrace(JsonSerializer.Serialize(transakcija));

            if (ModelState.IsValid)

            {

                try

                {

                    ctx.Add(transakcija);
                    ctx.SaveChanges();
                    logger.LogInformation(new EventId(1000), $"Transakcija {transakcija.OpisTransakcije} dodan.");


                    TempData[RPPP_WebApp.Constants.Message] = "Transakcija dodana.";

                    TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;

                    return RedirectToAction(nameof(Index));

                }

                catch (Exception exc)

                {

                    logger.LogError("Pogreška prilikom dodavanja nove transakcije: {0}", exc.CompleteExceptionMessage());

                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());

                    return View(transakcija);

                }

            }

            else

            {

                return View(transakcija);

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

            var transakcija = ctx.Transakcijas.Find(id);

            if (transakcija != null)

            {

                try

                {

                    ctx.Remove(transakcija);

                    ctx.SaveChanges();

                    logger.LogInformation($"Transakcija uspješno obrisana");

                    TempData[RPPP_WebApp.Constants.Message] = $"Transakcija uspješno obrisana";

                    TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;

                }

                catch (Exception exc)

                {

                    TempData[RPPP_WebApp.Constants.Message] = "Pogreška prilikom brisanja transakcije: " + exc.CompleteExceptionMessage();

                    TempData[RPPP_WebApp.Constants.ErrorOccurred] = true;

                    logger.LogError("Pogreška prilikom brisanja transakcije: " + exc.CompleteExceptionMessage());

                }

            }

            else

            {

                logger.LogWarning("Ne postoji transakcija s ID-om: {0} ", id);

                TempData[RPPP_WebApp.Constants.Message] = "Ne postoji transakcija s ID-om: " + id;

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

            var transakcija = ctx.Transakcijas.AsNoTracking().Where(t => t.IdTransakcija == id).SingleOrDefault();

            if (transakcija == null)

            {

                logger.LogWarning("Ne postoji transakcija s ID-om: {0} ", id);

                return NotFound("Ne postoji transakcija s ID-om: " + id);

            }

            else

            {

                ViewBag.Page = page;

                ViewBag.Sort = sort;

                ViewBag.Ascending = ascending;
                await PrepareDropDownLists();
                return View(transakcija);

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
                Transakcija transakcija = await ctx.Transakcijas

                                      .Where(t => t.IdTransakcija == id).FirstOrDefaultAsync(); if (transakcija == null)

                {
                    return NotFound("Neispravan ID transakcije: " + id);

                }

                if (await TryUpdateModelAsync<Transakcija>(transakcija, "",

                     t=> t.IdTransakcija,t => t.Ibanposiljatelja,t => t.Ibanprimatelja, t => t.OpisTransakcije, t => t.Iznos,t => t.IdVrstaTransakcije))
                {
                    ViewBag.Page = page; ViewBag.Sort = sort; ViewBag.Ascending = ascending;


                    try
                    {
                        await ctx.SaveChangesAsync();

                        TempData[RPPP_WebApp.Constants.Message] = "Transakcija ažurirana.";

                        TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;

                        return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });

                    }

                    catch (Exception exc)

                    {

                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());

                        return View(transakcija);

                    }

                }

                else

                {

                    ModelState.AddModelError(string.Empty, "Podatke o transakciji nije moguće povezati s formom");

                    return View(transakcija);

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
        /// Stvaranje master-detail podataka
        /// </summary>
        /// <param name="transakcija">Sluzi za stvaranje forme po parametrima objekta</param>
        /// <param name="iban">Sluzi za stvaranje forme po parametrima objekta</param>
        /// <returns>Vraca master-detail prikaz s novim podatkom</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create2(Transakcija transakcija, string iban)
        {
            logger.LogTrace(JsonSerializer.Serialize(transakcija));

            if (ModelState.IsValid)
            {
                try
                {
                    var racun = ctx.RacunProjekta.Find(iban);
                    transakcija.Ibans.Add(racun);
                    ctx.Add(transakcija);
                    ctx.SaveChanges();
                    logger.LogInformation(new EventId(1000), $"Transakcija {transakcija.OpisTransakcije} dodana.");

                    TempData[RPPP_WebApp.Constants.Message] = "Transakcija dodana.";
                    TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;

                    // Preusmjeravanje na Edit akciju s parametrom iban
                    return RedirectToAction("Edit", "RacunProjekta", new { iban});
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanja nove transakcije: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());

                    return View(transakcija);
                }
            }
            else
            {
                return View(transakcija);
            }
        }

        /// <summary>
        /// Spremanje master-detail podatka nakon promjena
        /// </summary>
        /// <param name="id">Id aktivnosti</param>
        /// <param name="page">Stranica na kojoj se nalazimo</param>
        /// <param name="Iban">Iban racuna koji se ureduje</param>
        /// <param name="sort">Vrijednost po kojoj se sortira</param>
        /// <param name="ascending">Redoslijed sortiranja</param>
        /// <returns>Vraca master-detail podatak</returns>
        [HttpPost, ActionName("Edit2")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update2(int id, int page, string Iban, int sort, bool ascending = true)
        {
            try
            {
                Transakcija transakcija = await ctx.Transakcijas
                    .Where(t => t.IdTransakcija == id).FirstOrDefaultAsync();

                if (transakcija == null)
                {
                    return NotFound("Neispravan ID transakcije: " + id);
                }

                if (await TryUpdateModelAsync<Transakcija>(transakcija, "",
                     t => t.IdTransakcija, t => t.Ibanposiljatelja, t => t.Ibanprimatelja, t => t.OpisTransakcije, t => t.Iznos, t => t.IdVrstaTransakcije))
                {
                    ViewBag.Page = page;
                    ViewBag.Sort = sort;
                    ViewBag.Ascending = ascending;

                    try
                    {
                        await ctx.SaveChangesAsync();

                        TempData[RPPP_WebApp.Constants.Message] = "Transakcija ažurirana.";
                        TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;

                        // Redirektaj na akciju Edit u kontroleru RacunProjekta s potrebnim parametrima
                        return RedirectToAction("Edit", "RacunProjekta", new { iban = Iban, page = page, sort = sort, ascending = ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View(transakcija);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o transakciji nije moguće povezati s formom");
                    return View(transakcija);
                }
            }
            catch (Exception exc)
            {
                TempData[RPPP_WebApp.Constants.Message] = exc.CompleteExceptionMessage();
                TempData[RPPP_WebApp.Constants.ErrorOccurred] = true;

                // Redirektaj na akciju Edit u kontroleru RacunProjekta s potrebnim parametrima
                return RedirectToAction("Edit", "RacunProjekta", new { iban = Iban, page = page, sort = sort, ascending = ascending });
            }
        }

        /// <summary>
        /// Brisanje master-detail podataka
        /// </summary>
        /// <param name="Iban">Iban racuna</param>
        /// <param name="id">Id transakcije</param>
        /// <returns>Vraca poslijednju aktivnu stranicu</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete2(string Iban, int id)
        {
            var transakcija = ctx.Transakcijas.Find(id);

            if (transakcija != null)
            {
                try
                {
                    foreach (var racun in ctx.RacunProjekta.Include(p => p.IdTransakcijas))
                    {
                        var zahtjevZaBrisanje = racun.IdTransakcijas.FirstOrDefault(z => z.IdTransakcija == id);
                        if (zahtjevZaBrisanje != null)
                        {
                            racun.IdTransakcijas.Remove(zahtjevZaBrisanje);
                            break;
                        }
                    }
                    ctx.Remove(transakcija);
                    ctx.SaveChanges();

                    logger.LogInformation($"Transakcija uspješno obrisana");
                    TempData[RPPP_WebApp.Constants.Message] = $"Transakcija uspješno obrisana";
                    TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[RPPP_WebApp.Constants.Message] = "Pogreška prilikom brisanja transakcije: " + exc.CompleteExceptionMessage();
                    TempData[RPPP_WebApp.Constants.ErrorOccurred] = true;
                    logger.LogError("Pogreška prilikom brisanja transakcije: " + exc.CompleteExceptionMessage());
                }
            }
            else
            {
                logger.LogWarning("Ne postoji transakcija s ID-om: {0} ", id);
                TempData[RPPP_WebApp.Constants.Message] = "Ne postoji transakcija s ID-om: " + id;
                TempData[RPPP_WebApp.Constants.ErrorOccurred] = true;
            }

            // Redirektaj na akciju Edit u kontroleru RacunProjekta s potrebnim parametrima
            return RedirectToAction("Edit", "RacunProjekta", new { iban = Iban,  });
        }



        private async Task PrepareDropDownLists()
        {           

            // Dropdown list for vrsteTransakcija using IDs only
            var vrsteTransakcija = await ctx.VrstaTransakcijes
                                    .OrderBy(p => p.IdVrstaTransakcije)
                                    .Select(p => new { p.IdVrstaTransakcije })
                                    .ToListAsync();

            ViewBag.vrstaTransakcije = new SelectList(vrsteTransakcija, nameof(VrstaTransakcije.IdVrstaTransakcije), nameof(VrstaTransakcije.IdVrstaTransakcije));
        }
    }

}
