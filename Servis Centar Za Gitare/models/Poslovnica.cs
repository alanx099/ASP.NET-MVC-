using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Servis_Centar_Za_Gitare.models
{
    public class Poslovnica
    {
        private List<ZapTehnicar> _tehnicari = new List<ZapTehnicar>();
        private List<Zaposlenik> _menadzeri = new List<Zaposlenik>();
        private List<Nalog> _nalozi = new List<Nalog>();
        private List<Stranka> _stranke = new List<Stranka>();
        private String _ime = string.Empty;
        private String _adresa = string.Empty;

        public Poslovnica()
        {
            _tehnicari = new List<ZapTehnicar>();
            _menadzeri = new List<Zaposlenik>();
            _nalozi = new List<Nalog>();
            _stranke = new List<Stranka>();
        }

        public Poslovnica(List<ZapTehnicar> tehnicari, List<Zaposlenik> menadzeri, List<Nalog> nalozi, List<Stranka> stranke, string ime, string adresa)
        {
            Tehnicari = tehnicari;
            Menadzeri = menadzeri;
            Nalozi = nalozi;
            Stranke = stranke;
            Ime = ime;
            Adresa = adresa;
        }

        public List<ZapTehnicar> Tehnicari
        {
            get { return _tehnicari; }
            set { _tehnicari = value; }
        }

        public List<Zaposlenik> Menadzeri
        {
            get { return _menadzeri; }
            set { _menadzeri = value; }
        }

        public List<Nalog> Nalozi
        {
            get { return _nalozi; }
            set { _nalozi = value; }
        }

        public List<Stranka> Stranke
        {
            get { return _stranke; }
            set { _stranke = value; }
        }

        public List<Gitara> Gitare
        {
            get { return _stranke.SelectMany(s => s.Gitare).ToList(); }
        }

        public String Ime
        {
            get { return _ime; }
            set { _ime = value; }
        }

        public String Adresa
        {
            get { return _adresa; }
            set { _adresa = value; }
        }
    }
}
