namespace RPPP_WebApp.Extensions.Selectors
{
    using RPPP_WebApp.Models;
    using System.Linq.Expressions;

        public static class ZadatakSort
        {
        /// <summary>
        /// Sortiranje podataka
        /// </summary>
        /// <param name="query">Query podataka za sortiranje</param>
        /// <param name="sort">Redni broj parametra po kojem se sortira</param>
        /// <param name="ascending">Redoslijed sortiranja</param>
        /// <returns>Vraca sortirane podatke</returns>
        public static IQueryable<Zadaci> ApplySort(this IQueryable<Zadaci> query, int sort, bool ascending)
            {
                Expression<Func<Zadaci, object>> orderSelector = sort switch
                {
                    1 => z => z.IdZadatak,
                    2 => z => z.Status,
                    3 => z => z.Aktivan,
                    4 => z => z.Opis,
                    5 => z => z.IdZahtjev,
                    6 => z => z.NositeljZadatka,
                    7 => z => z.IdPrioritetZadatka,
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
