namespace RPPP_WebApp.Extensions.Selectors
{
    using RPPP_WebApp.Models;
    using System.Linq.Expressions;

        public static class UlogaSort
        {
        /// <summary>
        /// Sortiranje podataka
        /// </summary>
        /// <param name="query">Query podataka za sortiranje</param>
        /// <param name="sort">Redni broj parametra po kojem se sortira</param>
        /// <param name="ascending">Redoslijed sortiranja</param>
        /// <returns>Vraca sortirane podatke</returns>
        public static IQueryable<Uloge> ApplySort(this IQueryable<Uloge> query, int sort, bool ascending)
            {
                Expression<Func<Uloge, object>> orderSelector = sort switch
                {
                    1 => u => u.IdUloga,
                    2 => u => u.IdVrstaUloga,
                    3 => u => u.OpisUloga,
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