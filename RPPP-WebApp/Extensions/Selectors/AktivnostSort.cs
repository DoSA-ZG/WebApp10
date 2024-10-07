namespace RPPP_WebApp.Extensions.Selectors
{
    using RPPP_WebApp.Models;
    using System.Linq.Expressions;
        public static class AktivnostSort
        {
        /// <summary>
        /// Sortiranje podataka
        /// </summary>
        /// <param name="query">Query podataka za sortiranje</param>
        /// <param name="sort">Redni broj parametra po kojem se sortira</param>
        /// <param name="ascending">Redoslijed sortiranja</param>
        /// <returns>Vraca sortirane podatke</returns>
            public static IQueryable<Aktivnost> ApplySort(this IQueryable<Aktivnost> query, int sort, bool ascending)
            {
                Expression<Func<Aktivnost, object>> orderSelector = sort switch
                {
                    1 => a => a.IdAktivnost,
                    2 => a => a.OpisAktivnosti,
                    3 => a => a.DatumPocetka,
                    4 => a => a.DatumZavrsetka,
                    5 => a => a.IdVrstaAktivnosti,
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
