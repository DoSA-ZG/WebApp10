namespace RPPP_WebApp.ViewModels
{
    public class TransakcijaViewModel
    {
        public int IdTransakcija { get; set; }

        public string Ibanposiljatelja { get; set; }

        public string Ibanprimatelja { get; set; }

        public string OpisTransakcije { get; set; }

        public string Iznos { get; set; }

        public int IdVrstaTransakcije { get; set; }
    }
}
