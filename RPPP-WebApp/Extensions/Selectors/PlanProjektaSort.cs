namespace RPPP_WebApp.Extensions.Selectors
{
    using RPPP_WebApp.Models;
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    public static class PlanProjektaSort
    {
        /// <summary>
        /// Sortiranje podataka
        /// </summary>
        /// <param name="query">Query podataka za sortiranje</param>
        /// <param name="sort">Redni broj parametra po kojem se sortira</param>
        /// <param name="ascending">Redoslijed sortiranja</param>
        /// <returns>Vraca sortirane podatke</returns>
        public static IQueryable<PlanProjektum> ApplySort(this IQueryable<PlanProjektum> query, int sort, bool ascending)
        {
            Expression<Func<PlanProjektum, object>> orderSelector = sort switch
            {
                1 => p => p.IdPlanProjekta,
                2 => p => p.PlaniraniPocetakZadatka,
                3 => p => p.StvarniPocetakZadatka,
                4 => p => p.PlaniraniZavrsetakZadatka,
                5 => p => p.StvarniZavrsetakZadatka,
                6 => p => p.IdProjekt,
                7 => p => p.IdVoditelj,
                _ => null
            };

            if (orderSelector != null)
            {
                query = ascending ?
                       query.OrderBy(orderSelector) :
                       query.OrderByDescending(orderSelector);
            }

            return query;
        }
    }
}
