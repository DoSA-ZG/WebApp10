using RPPP_WebApp.Models;
using System.Linq.Expressions;

namespace RPPP_WebApp.Extensions.Selectors
{
    public static class SudionikSort
    {
        /// <summary>
        /// Sortiranje podataka
        /// </summary>
        /// <param name="query">Query podataka za sortiranje</param>
        /// <param name="sort">Redni broj parametra po kojem se sortira</param>
        /// <param name="ascending">Redoslijed sortiranja</param>
        /// <returns>Vraca sortirane podatke</returns>
        public static IQueryable<Sudionik> ApplySort(this IQueryable<Sudionik> query, int sort, bool ascending)
        {
            Expression<Func<Sudionik, object>> orderSelector = sort switch
            {
                1 => s => s.IdSudionik,
                2 => s => s.Email,
                3 => s => s.Kontakt,
                4 => s => s.AdresaUreda,
                5 => s => s.IdFirmaNavigation.NazivFirma,
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