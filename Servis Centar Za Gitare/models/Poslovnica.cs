using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Servis_Centar_Za_Gitare.models
{
    public class Poslovnica
    {
        private long _id;
        private List<ZapTehnicar> _tehnicari = new List<ZapTehnicar>();
        private List<Zaposlenik> _menadzeri = new List<Zaposlenik>();
        private List<Nalog> _nalozi = new List<Nalog>();
        private List<Stranka> _stranke = new List<Stranka>();
        private List<Zaposlenik> _zaposlenici = new List<Zaposlenik>();
        private String _ime = string.Empty;
        private String _adresa = string.Empty;

        public Poslovnica()
        {
            _tehnicari = new List<ZapTehnicar>();
            _menadzeri = new List<Zaposlenik>();
            _nalozi = new List<Nalog>();
            _stranke = new List<Stranka>();
            _zaposlenici = new List<Zaposlenik>();
        }

        public Poslovnica(List<ZapTehnicar> tehnicari, List<Zaposlenik> menadzeri, List<Nalog> nalozi, List<Stranka> stranke, string ime, string adresa)
        {
            Tehnicari = tehnicari;
            Menadzeri = menadzeri;
            Zaposlenici = tehnicari.Cast<Zaposlenik>().Concat(menadzeri).ToList();
            Nalozi = nalozi;
            Stranke = stranke;
            Ime = ime;
            Adresa = adresa;
        }

        [Key]
        public long Id
        {
            get { return _id; }
            set { _id = value; }
        }

        [NotMapped]
        public List<ZapTehnicar> Tehnicari
        {
            get { return _tehnicari; }
            set { _tehnicari = value; }
        }

        [NotMapped]
        public List<Zaposlenik> Menadzeri
        {
            get { return _menadzeri; }
            set { _menadzeri = value; }
        }

        public virtual ICollection<Zaposlenik> Zaposlenici
        {
            get { return _zaposlenici; }
            set { _zaposlenici = value.ToList(); }
        }

        public virtual ICollection<Nalog> Nalozi
        {
            get { return _nalozi; }
            set { _nalozi = value.ToList(); }
        }

        public virtual ICollection<Stranka> Stranke
        {
            get { return _stranke; }
            set { _stranke = value.ToList(); }
        }

        [NotMapped]
        public List<Gitara> Gitare
        {
            get { return _stranke.SelectMany(s => s.Gitare).ToList(); }
        }

        [Required]
        [MaxLength(120)]
        public String Ime
        {
            get { return _ime; }
            set { _ime = value; }
        }

        [Required]
        [MaxLength(200)]
        public String Adresa
        {
            get { return _adresa; }
            set { _adresa = value; }
        }
    }
}
