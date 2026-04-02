using System;
using System.Collections.Generic;
using System.Text;
using Servis_Centar_Za_Gitare.enums;

namespace Servis_Centar_Za_Gitare.models
{
    public class Znanje
    {
        private TipGitareEnum _tipGitare;
        private VrstaPopravkaEnum _vrstaPopravka;

        public TipGitareEnum TipGitare
        {
            get { return _tipGitare; }
            set { _tipGitare = value; }
        }

        public VrstaPopravkaEnum VrstaPopravka
        {
            get { return _vrstaPopravka; }
            set { _vrstaPopravka = value; }
        }

        public Znanje() { }

        public Znanje(TipGitareEnum tipGitare, VrstaPopravkaEnum vrstaPopravka)
        {
            TipGitare = tipGitare;
            VrstaPopravka = vrstaPopravka;
        }
    }
}
