using System;
using System.Collections.Generic;
using System.Text;

namespace Servis_Centar_Za_Gitare.models
{
    public class ZapTehnicar: Zaposlenik
    {
        private List<Znanje> _znanja = new List<Znanje>();

        public List<Znanje> Znanja
        {
            get { return _znanja; }
            set { _znanja = value; }
        }

        public ZapTehnicar() { }

        public ZapTehnicar(List<Znanje> znanja)
        {
            Znanja = znanja;
        }

        public ZapTehnicar(long id, string ime, string prezime, string email, string brojTelefona, string adresa, string datumZaposlenja, double placa, List<Znanje> znanja)
            : base(id, ime, prezime, email, brojTelefona, adresa, datumZaposlenja, placa)
        {
            Znanja = znanja;
        }
    }
}
