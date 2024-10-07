namespace RPPP_WebApp.Extensions.Selectors
{
    using RPPP_WebApp.Models;
    using System.Linq.Expressions;

        public static class ProjektniDokuemntSort
        {
        /// <summary>
        /// Sortiranje podataka
        /// </summary>
        /// <param name="query">Query podataka za sortiranje</param>
        /// <param name="sort">Redni broj parametra po kojem se sortira</param>
        /// <param name="ascending">Redoslijed sortiranja</param>
        /// <returns>Vraca sortirane podatke</returns>
        public static IQueryable<ProjektnaDokumentacija> ApplySort(this IQueryable<ProjektnaDokumentacija> query, int sort, bool ascending)
            {
                Expression<Func<ProjektnaDokumentacija, object>> orderSelector = sort switch
                {
                    1 => pd => pd.IdDokument,
                    2 => pd => pd.NazivDokumenta,
                    3 => pd => pd.IdVrstaDokument,
                    4 => pd => pd.FormatDokumenta,
                    5 => pd => pd.IdProjekt,
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
