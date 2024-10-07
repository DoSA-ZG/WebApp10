﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;


namespace RPPP_WebApp.Controllers
{


    public class VrstaUlogeController : Controller
    {
        private readonly ProjektContext ctx;
        private readonly ILogger<UlogaController> logger;
        private readonly AppSettings appSettings;

        public VrstaUlogeController(ProjektContext ctx, IOptionsSnapshot<AppSettings> options, ILogger<VrstaUlogeController> logger)
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

            var query = ctx.VrstaUloges.AsNoTracking();

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



            var uloge = query
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .ToList();

            var model = new VrstaUlogeViewModel
            {
                vrstaUloges = uloge,
                PagingInfo = pagingInfo
            };

            return View(model);
        }
    }
}