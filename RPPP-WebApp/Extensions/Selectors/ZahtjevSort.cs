namespace RPPP_WebApp.Extensions.Selectors
{
    using RPPP_WebApp.Models;
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    public static class ZahtjevSort
    {
        /// <summary>
        /// Sortiranje podataka
        /// </summary>
        /// <param name="query">Query podataka za sortiranje</param>
        /// <param name="sort">Redni broj parametra po kojem se sortira</param>
        /// <param name="ascending">Redoslijed sortiranja</param>
        /// <returns>Vraca sortirane podatke</returns>
        public static IQueryable<Zahtjev> ApplySort(this IQueryable<Zahtjev> query, int sort, bool ascending)
        {
            Expression<Func<Zahtjev, object>> orderSelector = sort switch
            {
                1 => z => z.IdZahtjev,
                2 => z => z.Opis,
                3 => z => z.Prioritet,
                4 => z => z.Vrsta,
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
