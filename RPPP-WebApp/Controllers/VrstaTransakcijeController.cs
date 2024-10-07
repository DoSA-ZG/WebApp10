using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using System.Linq;

namespace RPPP_WebApp.Controllers
{
    public class VrstaTransakcijeController : Controller
    {
        private readonly ProjektContext ctx;
        private readonly ILogger<VrstaTransakcijeController> logger; 
        private readonly AppSettings appSettings;

        public VrstaTransakcijeController(ProjektContext ctx, IOptionsSnapshot<AppSettings> options, ILogger<VrstaTransakcijeController> logger)
        {
            this.ctx = ctx;
            appSettings = options.Value;
            this.logger = logger;
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

            var query = ctx.VrstaTransakcijes.AsNoTracking();

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

            var transakcije = query
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .ToList();

            var model = new VrstaTransakcijaViewModel
            {
                VrstaTransakcijes = transakcije,
                PagingInfo = pagingInfo
            };

            return View(model);
        }
    }
}