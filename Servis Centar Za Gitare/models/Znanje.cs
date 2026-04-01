using System;
using System.Collections.Generic;
using System.Text;

namespace Servis_Centar_Za_Gitare.models
{
    public class Znanje
    {
        private string _tipGitare;
        private string _vrstaPopravka;

        public string TipGitare
        {
            get { return _tipGitare; }
            set { _tipGitare = value; }
        }

        public string VrstaPopravka
        {
            get { return _vrstaPopravka; }
            set { _vrstaPopravka = value; }
        }

        public Znanje() { }

        public Znanje(string tipGitare, string vrstaPopravka)
        {
            TipGitare = tipGitare;
            VrstaPopravka = vrstaPopravka;
        }
    }
}
