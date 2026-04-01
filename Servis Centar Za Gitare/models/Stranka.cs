using System;
using System.Collections.Generic;
using System.Text;

namespace Servis_Centar_Za_Gitare.models
{
    public class Stranka
    {
        private long _id;
        private String _ime;
        private String _prezime;
        private String _email;
        private String _brojTelefona;
        private String _adresa;
        private String _datumRegistracije;
        private String _napomena;
        private List<Gitara> _gitare;

        public Stranka()
        {
            _gitare = new List<Gitara>();
        }

    public Stranka(long id, string ime, string prezime, string email, string brojTelefona, string adresa, string datumRegistracije, string napomena, List<Gitara> gitare)
    {
        Id = id;
        Ime = ime;
        Prezime = prezime;
        Email = email;
        BrojTelefona = brojTelefona;
        Adresa = adresa;
        DatumRegistracije = datumRegistracije;
        Napomena = napomena;
        Gitare = gitare;
    }

        public long Id { get => _id; set => _id = value; }
        public string Ime { get => _ime; set => _ime = value; }
        public string Prezime { get => _prezime; set => _prezime = value; }
        public string Email { get => _email; set => _email = value; }
        public string BrojTelefona { get => _brojTelefona; set => _brojTelefona = value; }
        public string Adresa { get => _adresa; set => _adresa = value; }
        public string DatumRegistracije { get => _datumRegistracije; set => _datumRegistracije = value; }
        public string Napomena { get => _napomena; set => _napomena = value; }
        public List<Gitara> Gitare { get => _gitare; set => _gitare = value; }
    }
}
