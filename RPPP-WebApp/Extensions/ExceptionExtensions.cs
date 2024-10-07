using System;
using System.Text;

namespace RPPP_WebApp.Extensions
{
    /// <summary>
    /// Razred sa pro�irenjima za iznimke
    /// </summary>
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Vra�a sadr�aj poruka cijele hijerarhije neke iznimke. Za predanu iznimku provjerava se postoji li unutarnja iznimka.
        /// Ako da, poruka unutanje iznimke dodaje se u rezultat te se dalje provjerava postoji li unutarnja iznimka unutarnje iznimke itd...    
        /// </summary>
        /// <param name="exc">Iznimka �ija se kompletna hijerarhija poruka treba ispisati</param>
        /// <returns>String formiran od poruka svih unutarnjih iznimki. Poruka svake iznimmke dodana je u novi redak</returns>
        public static string CompleteExceptionMessage(this Exception exc)
        {
            StringBuilder sb = new StringBuilder();
            while (exc != null)
            {
                sb.AppendLine(exc.Message);
                exc = exc.InnerException;
            }
            return sb.ToString();
        }
    }
}
