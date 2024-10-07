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
    public class RacunProjektaController : Controller

    {

        private readonly ProjektContext ctx;

        private readonly ILogger<RacunProjektaController> logger;

        private readonly AppSettings appSettings;

        public RacunProjektaController(ProjektContext ctx, IOptionsSnapshot<AppSettings> options, ILogger<RacunProjektaController> logger)

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

            int pageSize = appSettings.PageSize;

            var query = ctx.RacunProjekta.AsNoTracking();

            int count = query.Count();

            if (count == 0)

            {

                logger.LogInformation("Ne postoji niti jedan račun projekta.");

                TempData[RPPP_WebApp.Constants.Message] = "Ne postoji niti jedan račun projekta.";

                TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;

                return RedirectToAction(nameof(Create));

            }

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

            query = query.ApplySort(sort, ascending);

            var racuniProjekta = query

                .Skip((page - 1) * pageSize)

                .Take(pageSize)
                .Include(t => t.IdTransakcijas)

                .ToList();

            var model = new RacuniProjektaViewModel

            {

                Racuni = racuniProjekta,

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

        public IActionResult Create(RacunProjektaViewModel model)

        {

            logger.LogTrace(JsonSerializer.Serialize(model));

            if (ModelState.IsValid)

            {

                try

                {

                    var projekt = ctx.Projekts.Include(p => p.RacunProjekta).FirstOrDefault(p => p.IdProjekt == model.IdProjekt);

                    if (projekt != null)

                    {

                        if (projekt.RacunProjekta == null)

                        {

                            projekt.RacunProjekta = new List<RacunProjektum>();

                        }

                        var noviRacunProjekta = new RacunProjektum

                        {

                            Iban = model.Iban,

                            NazivProjekta = model.NazivProjekta,

                            StanjeRacuna = model.StanjeRacuna

                        };

                        projekt.RacunProjekta.Add(noviRacunProjekta);

                        ctx.SaveChanges();

                        logger.LogInformation(new EventId(1000), $"Račun projekta {noviRacunProjekta.NazivProjekta} dodan.");

                        TempData[RPPP_WebApp.Constants.Message] = $"Račun projekta {noviRacunProjekta.NazivProjekta} dodan.";

                        TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;

                        return RedirectToAction(nameof(Index));

                    }

                    else

                    {



                        TempData[RPPP_WebApp.Constants.Message] = "Projekt nije pronađen.";

                        TempData[RPPP_WebApp.Constants.ErrorOccurred] = true;

                    }

                }

                catch (Exception exc)

                {

                    logger.LogError("Pogreška prilikom dodavanja novog računa projekta: {0}", exc.CompleteExceptionMessage());

                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());

                }

            }

            return View(model);

        }

        /// <summary>
        /// Metoda za brisanje.
        /// </summary>
        /// <param name="iban">Iban racuna za koji se brisu podaci</param>
        /// <param name="page">Stranica na kojoj se nalazimo</param>
        /// <param name="sort">Vrijednost po kojoj se sortira</param>
        /// <param name="ascending">Redoslijed sortiranja</param>
        /// <returns>Vraca trenutnu stranicu nakon brisanja</returns>
        /// 
        [HttpPost]

        [ValidateAntiForgeryToken]

        public IActionResult Delete(string iban, int page, int sort, bool ascending = true)

        {

            var racunProjekta = ctx.RacunProjekta.Find(iban);

            if (racunProjekta != null)

            {

                try

                {



                    foreach (var projekt in ctx.Projekts.Include(p => p.RacunProjekta))

                    {

                        var racunZaBrisanje = projekt.RacunProjekta.FirstOrDefault(rp => rp.Iban == iban);

                        if (racunZaBrisanje != null)

                        {

                            projekt.RacunProjekta.Remove(racunZaBrisanje);

                            break;

                        }

                    }



                    ctx.Remove(racunProjekta);

                    ctx.SaveChanges();

                    logger.LogInformation($"Račun projekta uspješno obrisan");

                    TempData[RPPP_WebApp.Constants.Message] = $"Račun projekta uspješno obrisan";

                    TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;

                }

                catch (Exception exc)

                {

                    TempData[RPPP_WebApp.Constants.Message] = "Pogreška prilikom brisanja računa projekta: " + exc.CompleteExceptionMessage();

                    TempData[RPPP_WebApp.Constants.ErrorOccurred] = true;

                    logger.LogError("Pogreška prilikom brisanja računa projekta: " + exc.CompleteExceptionMessage());

                }

            }

            else

            {

                logger.LogWarning("Ne postoji račun projekta s IBAN-om: {0} ", iban);

                TempData[RPPP_WebApp.Constants.Message] = "Ne postoji račun projekta s IBAN-om: " + iban;

                TempData[RPPP_WebApp.Constants.ErrorOccurred] = true;

            }

            return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });

        }
        /// <summary>
        /// Uredivanje podatka
        /// </summary>
        /// <param name="iban">Iban racuna koji se ureduje</param>
        /// <param name="page">Stranica na kojoj se nalazimo</param>
        /// <param name="sort">Vrijednost po kojoj se sortira</param>
        /// <param name="ascending">Redoslijed sortiranja</param>
        /// <returns>Vraca formu sa vrijednostima uredenog podatka</returns>
        [HttpGet]
        public async Task<IActionResult> Edit(string iban, int page, int sort, bool ascending = true)
        {
            var racun = await ctx.RacunProjekta
                                 .Where(d => d.Iban == iban)
                                 .Select(d => new RacunProjektaViewModel
                                 {
                                     Iban = d.Iban,
                                     NazivProjekta = d.NazivProjekta,
                                     StanjeRacuna = d.StanjeRacuna,
                                     IdProjekt = d.IdProjekt,
                                     IdTransakcijas = d.IdTransakcijas // Dodano pridruživanje transakcija
                                                     .OrderBy(t => t.IdTransakcija)
                                                     .Select(t => new TransakcijaViewModel
                                                     {
                                                         IdTransakcija = t.IdTransakcija,
                                                         Ibanposiljatelja = t.Ibanposiljatelja,
                                                         Ibanprimatelja = t.Ibanprimatelja,
                                                         OpisTransakcije = t.OpisTransakcije,
                                                         Iznos = t.Iznos,
                                                         IdVrstaTransakcije = t.IdVrstaTransakcije
                                                     })
                                                     .ToList()
                                 })
                                 .FirstOrDefaultAsync();

            if (racun == null)
            {
                return NotFound($"Dokument {iban} ne postoji");
            }
            else
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;

                return View(racun);
            }
        }

        /// <summary>
        /// Spremanje promjena uredivanog podatka
        /// </summary>
        /// <param name="racunProjekta">Model po kojem se stvaraju parametri za uredivanje podatka</param>
        /// <param name="page">Stranica na kojoj se nalazimo</param>
        /// <param name="sort">Vrijednost po kojoj se sortira</param>
        /// <param name="ascending">Redoslijed sortiranja</param>
        /// <returns>Vraca promijenjeni podatak</returns>
        [HttpPost, ActionName("Edit")]

        [ValidateAntiForgeryToken]

        public IActionResult Update(RacunProjektum racunProjekta, int page, int sort, bool ascending = true)
        {
            try
            {
                ctx.Update(racunProjekta);
                ctx.SaveChanges();

                TempData[RPPP_WebApp.Constants.Message] = "Račun projekta ažuriran.";
                TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;

                // Vraćanje na prethodnu stranicu
                return RedirectToAction("Edit", new { iban = racunProjekta.Iban, page = page, sort = sort, ascending = ascending });
            }
            catch (Exception exc)
            {
                logger.LogError("Pogreška prilikom ažuriranja računa projekta: {0}", exc.CompleteExceptionMessage());
                ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());

                // Ako dođe do greške, vratite se na prethodnu stranicu
                return RedirectToAction("Edit", new { iban = racunProjekta.Iban, page = page, sort = sort, ascending = ascending });
            }
        }
        /// <summary>
        /// Master-detail prikaz
        /// </summary>
        /// <param name="iban">Iban racuna koji se prikazuje</param>
        /// <param name="page">Stranica na kojoj se nalazimo</param>
        /// <param name="sort">Vrijednost po kojoj se sortira</param>
        /// <param name="ascending">Redoslijed sortiranja</param>
        /// <returns>Vraca master-detail prikaz podataka</returns>
        public IActionResult STP(string iban, int page = 1, int sort = 1, bool ascending = true)
        {

            int pageSize = appSettings.PageSize;


            var query = ctx.RacunProjekta.AsNoTracking().Where(r => r.Iban == iban);


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


            var racuniProjekta = query

                .Skip((page - 1) * pageSize)

                .Take(pageSize)
                .Include(t => t.IdTransakcijas)
                .ToList();


            var model = new RacuniProjektaViewModel

            {

                Racuni = racuniProjekta,

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