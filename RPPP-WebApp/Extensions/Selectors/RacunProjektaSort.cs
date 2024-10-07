namespace RPPP_WebApp.Extensions.Selectors
{
    using RPPP_WebApp.Models;
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    public static class RacunProjektaSort
    {
        /// <summary>
        /// Sortiranje podataka
        /// </summary>
        /// <param name="query">Query podataka za sortiranje</param>
        /// <param name="sort">Redni broj parametra po kojem se sortira</param>
        /// <param name="ascending">Redoslijed sortiranja</param>
        /// <returns>Vraca sortirane podatke</returns>
        public static IQueryable<RacunProjektum> ApplySort(this IQueryable<RacunProjektum> query, int sort, bool ascending)
        {
            Expression<Func<RacunProjektum, object>> orderSelector = sort switch
            {
                1 => rp => rp.Iban,
                2 => rp => rp.NazivProjekta,
                3 => rp => rp.StanjeRacuna,
                4 => rp => rp.IdProjekt,
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