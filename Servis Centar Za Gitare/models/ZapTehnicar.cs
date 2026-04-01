using System;
using System.Collections.Generic;
using System.Text;

namespace Servis_Centar_Za_Gitare.models
{
    public class ZapTehnicar: Zaposlenik
    {
        private List<Znanje> _znanja;

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
    }
}
