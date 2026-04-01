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
        }

        public Stranka(long id, string ime, string prezime, string email, string brojTelefona, string adresa, string datumRegistracije, string napomena, List<Gitara> gitare)
        {
            _id = id;
            Ime = ime;
            _prezime = prezime;
            _email = email;
            _brojTelefona = brojTelefona;
            _adresa = adresa;
            _datumRegistracije = datumRegistracije;
            _napomena = napomena;
            _gitare = gitare;
        }

        public string Ime { get => _ime; set => _ime = value; }
        public string Prezime { get => _prezime; set => _prezime = value; }
    }
}
