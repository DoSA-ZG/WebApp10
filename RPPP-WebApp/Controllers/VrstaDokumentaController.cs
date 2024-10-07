using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Threading.Tasks;

namespace RPPP_WebApp.Controllers
{


    public class VrstaDokumentaController : Controller
    {
        private readonly ProjektContext ctx;
        private readonly ILogger<VrstaDokumentaController> logger;
        private readonly AppSettings appSettings;

        public VrstaDokumentaController(ProjektContext ctx, IOptionsSnapshot<AppSettings> options, ILogger<VrstaDokumentaController> logger)
        {
            this.ctx = ctx;

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

            var query = ctx.VrstaDokumenta.AsNoTracking();

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



            var vrste = query
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .ToList();

            var model = new VrsteDokumenataViewModel
            {
                vrsteDokumenata = vrste,
                PagingInfo = pagingInfo
            };

            return View(model);
        }
    }
}