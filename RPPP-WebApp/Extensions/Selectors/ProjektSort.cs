namespace RPPP_WebApp.Extensions.Selectors
{
    using RPPP_WebApp.Models;
    using System.Linq.Expressions;

        public static class ProjektSort
        {
        /// <summary>
        /// Sortiranje podataka
        /// </summary>
        /// <param name="query">Query podataka za sortiranje</param>
        /// <param name="sort">Redni broj parametra po kojem se sortira</param>
        /// <param name="ascending">Redoslijed sortiranja</param>
        /// <returns>Vraca sortirane podatke</returns>
        public static IQueryable<Projekt> ApplySort(this IQueryable<Projekt> query, int sort, bool ascending)
            {
                Expression<Func<Projekt, object>> orderSelector = sort switch
                {
                    1 => p => p.IdProjekt,
                    2 => p => p.KraticaProjekta,
                    3 => p => p.NazivProjekta,
                    4 => p => p.DatumPocetka,
                    5 => p => p.DatumZavrsetka,
                    6 => p => p.IdVrstaProjekta,
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
