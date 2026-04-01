using System;
using System.Collections.Generic;
using System.Text;

namespace Servis_Centar_Za_Gitare.models
{
    public class Zaposlenik
    {
        private long _id;
        private String _ime;
        private String _prezime;
        private String _email;
        private String _brojTelefona;
        private String _adresa;
        private String _datumZaposlenja;
        private double _placa;

        public Zaposlenik()
        {
        }

        public Zaposlenik(long id, string ime, string prezime, string email, string brojTelefona, string adresa, string datumZaposlenja, double placa)
        {
            Id = id;
            Ime = ime;
            Prezime = prezime;
            Email = email;
            BrojTelefona = brojTelefona;
            Adresa = adresa;
            DatumZaposlenja = datumZaposlenja;
            Placa = placa; 
        }

        public long Id { get => _id; set => _id = value; }
        public string Ime { get => _ime; set => _ime = value; }
        public string Prezime { get => _prezime; set => _prezime = value; }
        public string Email { get => _email; set => _email = value; }
        public string BrojTelefona { get => _brojTelefona; set => _brojTelefona = value; }
        public string Adresa { get => _adresa; set => _adresa = value; }
        public string DatumZaposlenja { get => _datumZaposlenja; set => _datumZaposlenja = value; }
        private double Placa { get => _placa; set => _placa = value; }
    }
}
