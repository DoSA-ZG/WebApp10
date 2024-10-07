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


    public class ProjektnaDokumentacijaController : Controller
    {
        private readonly ProjektContext ctx;
        private readonly ILogger<ProjektnaDokumentacijaController> logger;
        private readonly AppSettings appSettings;

        public ProjektnaDokumentacijaController(ProjektContext ctx, IOptionsSnapshot<AppSettings> options, ILogger<ProjektnaDokumentacijaController> logger)
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

            var query = ctx.ProjektnaDokumentacijas.AsNoTracking();

            int count = query.Count();
            if (count == 0)
            {
                logger.LogInformation("Ne postoji nijedan dokument");
                TempData[RPPP_WebApp.Constants.Message] = "Ne postoji niti jedan dokument.";
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

            var projektnaDokumentacija = query
                .Skip((page - 1) * pagesize)
                .Include(p => p.IdProjektNavigation)
                .Include(p => p.IdVrstaDokumentNavigation)
                .Take(pagesize)
                .ToList();

            var model = new ProjektnaDokumentacijaViewModel
            {
                ProjektnaDokumentacija = projektnaDokumentacija,
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
            var vrste = await ctx.VrstaDokumenta
                                .OrderBy(v => v.IdVrstaDokument)
                                .Select(v => new {v.IdVrstaDokument, v.NazivVrste})
                                .ToListAsync();
            ViewBag.Dokumenti = new SelectList(vrste, nameof(VrstaDokumentum.IdVrstaDokument), nameof(VrstaDokumentum.NazivVrste));
            var projekti = await ctx.Projekts
                                .OrderBy(p => p.IdProjekt)
                                .Select(p =>  new {p.IdProjekt, p.NazivProjekta})
                                .ToListAsync();
            ViewBag.Projekti = new SelectList(projekti, nameof(Projekt.IdProjekt), nameof(Projekt.NazivProjekta));
        }
        /// <summary>
        /// Stvaranje novog podatka
        /// </summary>
        /// <param name="model">Sluzi za stvaranje parametara forme</param>
        /// <returns>Vraca rezultat stvaranja novog podatka</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProjektnaDokumentacija model)
        {
            logger.LogTrace(JsonSerializer.Serialize(model));
            if (ModelState.IsValid)
            {
                try
                {
                    if (model.IdDokument < 0)
                    {
                        Console.WriteLine("IdDokumenta mora biti pozitivan!");
                        ModelState.AddModelError(string.Empty, "IdDokumenta mora biti pozitivan!");
                        return RedirectToAction("Create", "ProjektnaDokumentacija", model);
                    }
                    var projekt = ctx.Projekts.Include(p => p.ProjektnaDokumentacijas).FirstOrDefault(p => p.IdProjekt == model.IdProjekt);
                    if(null != ctx.ProjektnaDokumentacijas.Find(model.IdDokument))
                    {
                        ModelState.AddModelError(string.Empty, $"Dokument s idjem: {model.IdDokument} veæ postoji!");
                        return RedirectToAction("Create", "ProjektnaDokumentacija", model);
                    }
                    if (null == ctx.VrstaDokumenta.Find(model.IdVrstaDokument))
                    {
                        ModelState.AddModelError(string.Empty, $"Neispravna vrsta dokumenta!");
                        return RedirectToAction("Create", "ProjektnaDokumentacija", model);
                    }
                    if (null == ctx.Projekts.Find(model.IdProjekt))
                    {
                        ModelState.AddModelError(string.Empty, $"Neispravan projekt id!");
                        return RedirectToAction("Create", "ProjektnaDokumentacija", model);
                    }
                    if(projekt.ProjektnaDokumentacijas == null)
                    {
                        projekt.ProjektnaDokumentacijas = new List<ProjektnaDokumentacija>();
                    }
                    var noviDokument = new ProjektnaDokumentacija { 
                        IdDokument = model.IdDokument,
                        NazivDokumenta = model.NazivDokumenta,
                        IdVrstaDokument = model.IdVrstaDokument,
                        FormatDokumenta = model.FormatDokumenta,
                        IdProjekt = model.IdProjekt
                    };
                    projekt.ProjektnaDokumentacijas.Add(noviDokument);
                    //ctx.Add(model);
                    ctx.SaveChanges();
                    logger.LogInformation(new EventId(1000), $"Dokument {noviDokument.NazivDokumenta} dodan.");

                    TempData[RPPP_WebApp.Constants.Message] = $"Dokument {noviDokument.NazivDokumenta} dodan.";
                    TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanje novog dokumenta: {0}", exc.CompleteExceptionMessage());
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

            var dokument = ctx.ProjektnaDokumentacijas.Find(id);

            if (dokument != null)
            {
                try
                {   
                    foreach (var projekt in ctx.Projekts.Include(p =>  p.ProjektnaDokumentacijas))
                    {
                        if (projekt.ProjektnaDokumentacijas.Remove(dokument))
                        {
                            break;
                        }
                    }

                    ctx.Remove(dokument);
                    ctx.SaveChanges();
                    logger.LogInformation($"Dokument uspješno obrisan");
                    TempData[RPPP_WebApp.Constants.Message] = $"Dokument  uspješno obrisan";
                    TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[RPPP_WebApp.Constants.Message] = "Pogreška prilikom brisanja dokumenta: " + exc.CompleteExceptionMessage();
                    TempData[RPPP_WebApp.Constants.ErrorOccurred] = true;
                    logger.LogError("Pogreška prilikom brisanja dokumenta: " + exc.CompleteExceptionMessage());
                }
            }
            else
            {
                logger.LogWarning("Ne postoji dokument s ID-om: {0} ", id);
                TempData[RPPP_WebApp.Constants.Message] = "Ne postoji dokument s ID-om: " + id;
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
        public async Task<IActionResult> Edit(int id, int page = 1, int sort = 1, bool ascending = true)
        {
            await PrepareDropDownLists();
            var dokument = ctx.ProjektnaDokumentacijas.AsNoTracking().Where(pd => pd.IdDokument == id).SingleOrDefault();
            if (dokument == null)
            {
                logger.LogWarning("Ne postoji dokument s ID-om: {0} ", id);
                return NotFound("Ne postoji dokument s ID-om: " + id);
            }
            else
            {
                ViewBag.Page = page;
                ViewBag.Sort = sort;
                ViewBag.Ascending = ascending;
                return View(dokument);
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
                ProjektnaDokumentacija dokument = await ctx.ProjektnaDokumentacijas
                                  .Where(pd => pd.IdDokument == id)
                                  .FirstOrDefaultAsync();
                if (dokument == null)
                {
                    return NotFound("Neispravan ID dokumenta: " + id);
                }

                if (await TryUpdateModelAsync<ProjektnaDokumentacija>(dokument, "",
                 pd => pd.NazivDokumenta, pd => pd.IdVrstaDokument, pd => pd.FormatDokumenta, pd => pd.IdProjekt))
                {
                    ViewBag.Page = page;
                    ViewBag.Sort = sort;
                    ViewBag.Ascending = ascending;
                    try
                    {
                        await ctx.SaveChangesAsync();
                        TempData[RPPP_WebApp.Constants.Message] = "Dokument ažuriran.";
                        TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;
                        return RedirectToAction(nameof(Index), new { page = page, sort = sort, ascending = ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View(dokument);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o dokumentu nije moguæe povezati s formom");
                    return View(dokument);
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
        /// <param name="dokument">Sluzi za stvaranje forme po parametrima objekta</param>
        /// <returns>Vraca master-detail prikaz s novim podatkom</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create2(ProjektnaDokumentacija dokument)
        {
            logger.LogTrace(JsonSerializer.Serialize(dokument));

            if (ModelState.IsValid)
            {
                try
                {
                    ctx.Add(dokument);
                    ctx.SaveChanges();
                    logger.LogInformation(new EventId(1000), $"Dokument {dokument.NazivDokumenta} dodan.");

                    TempData[RPPP_WebApp.Constants.Message] = $"Dokument {dokument.NazivDokumenta} dodan.";
                    TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;


                    int idDokument = dokument.IdProjekt;


                    return RedirectToAction("Edit", "Projekti", new { id = idDokument });
                }
                catch (Exception exc)
                {
                    logger.LogError("Pogreška prilikom dodavanje novog dokumenta: {0}", exc.CompleteExceptionMessage());
                    ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());

                    return View(dokument);
                }
            }
            else
            {
                return View(dokument);
            }
        }
        /// <summary>
        /// Brisanje master-detail podataka
        /// </summary>
        /// <param name="id">Id projektne dokumentacije</param>
        /// <returns>Vraca poslijednju aktivnu stranicu</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete2(int id)
        {
            var dokument = ctx.ProjektnaDokumentacijas.Find(id);

            if (dokument != null)
            {
                try
                {
                    ctx.Remove(dokument);
                    ctx.SaveChanges();
                    logger.LogInformation($"Dokument uspješno obrisan");
                    TempData[RPPP_WebApp.Constants.Message] = $"Dokument uspješno obrisan";
                    TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;
                }
                catch (Exception exc)
                {
                    TempData[RPPP_WebApp.Constants.Message] = "Pogreška prilikom brisanja dokumenta: " + exc.CompleteExceptionMessage();
                    TempData[RPPP_WebApp.Constants.ErrorOccurred] = true;
                    logger.LogError("Pogreška prilikom brisanja dokumenta: " + exc.CompleteExceptionMessage());
                }
            }
            else
            {
                logger.LogWarning("Ne postoji dokument s ID-om: {0} ", id);
                TempData[RPPP_WebApp.Constants.Message] = "Ne postoji dokument s ID-om: " + id;
                TempData[RPPP_WebApp.Constants.ErrorOccurred] = true;
            }


            return RedirectToAction("Edit", "Projekti", new { id = dokument.IdProjekt });
        }
        /// <summary>
        /// Spremanje master-detail podatka nakon promjena
        /// </summary>
        /// <param name="id">Id podatka</param>
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
                ProjektnaDokumentacija dokument = await ctx.ProjektnaDokumentacijas
                                     .Where(z => z.IdDokument == id)
                                     .FirstOrDefaultAsync();
                if (dokument == null)
                {
                    return NotFound("Neispravan ID dokumenta: " + id);
                }

                if (await TryUpdateModelAsync<ProjektnaDokumentacija>(dokument, "",
                    z => z.IdDokument, z => z.NazivDokumenta, z => z.IdVrstaDokument, z => z.FormatDokumenta, z => z.IdProjekt))
                {
                    ViewBag.Page = page;
                    ViewBag.Sort = sort;
                    ViewBag.Ascending = ascending;
                    try
                    {
                        await ctx.SaveChangesAsync();
                        TempData[RPPP_WebApp.Constants.Message] = "Dokument ažuriran.";
                        TempData[RPPP_WebApp.Constants.ErrorOccurred] = false;


                        return RedirectToAction("Edit", "Projekti", new { id = dokument.IdProjekt, page = page, sort = sort, ascending = ascending });
                    }
                    catch (Exception exc)
                    {
                        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
                        return View(dokument);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Podatke o dokumentu nije moguæe povezati s formom");
                    return View(dokument);
                }
            }
            catch (Exception exc)
            {
                TempData[RPPP_WebApp.Constants.Message] = exc.CompleteExceptionMessage();
                TempData[RPPP_WebApp.Constants.ErrorOccurred] = true;


                return RedirectToAction("Edit", "Projekti", new { id = id, page = page, sort = sort, ascending = ascending });
            }
        }

    }
}
