namespace RPPP_WebApp.Extensions.Selectors
    {
        using global::RPPP_WebApp.Models;
        using System;
        using System.Linq;
        using System.Linq.Expressions;

        public static class TransakcijeSort
        {
        /// <summary>
        /// Sortiranje podataka
        /// </summary>
        /// <param name="query">Query podataka za sortiranje</param>
        /// <param name="sort">Redni broj parametra po kojem se sortira</param>
        /// <param name="ascending">Redoslijed sortiranja</param>
        /// <returns>Vraca sortirane podatke</returns>
        public static IQueryable<Transakcija> ApplySort(this IQueryable<Transakcija> query,int sort, bool ascending)
            {
                Expression<Func<Transakcija, object>> orderSelector = sort switch
                {
                    1 => t => t.IdTransakcija,
                    2 => t => t.Ibanposiljatelja,
                    3 => t => t.OpisTransakcije,
                    4 => t => t.Iznos,
                    5 => t => t.IdVrstaTransakcije,
                    6 => t => t.Ibanprimatelja,
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

