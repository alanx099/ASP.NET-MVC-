using System;
using System.Collections.Generic;
using System.Text;

namespace Servis_Centar_Za_Gitare.models
{
    public class Gitara
    {
        private long _id;
        private String _serijskiBroj;
        private String _marka;
        private String _brojZica;
        private String _tipGitare;
        private DateTime _datumZaprimanja;
        private long _kupacId;

        public Gitara()
        {
        }

        public Gitara(long id, string serijskiBroj, string marka, string brojZica, string tipGitare, DateTime datumZaprimanja, long kupacId)
        {
            Id = id;
            SerijskiBroj = serijskiBroj;
            Marka = marka;
            BrojZica = brojZica;
            TipGitare = tipGitare;
            DatumZaprimanja = datumZaprimanja;
            KupacId = kupacId;
        }

        public long Id { get => _id; set => _id = value; }
        public string SerijskiBroj { get => _serijskiBroj; set => _serijskiBroj = value; }
        public string Marka { get => _marka; set => _marka = value; }
        public string BrojZica { get => _brojZica; set => _brojZica = value; }
        public string TipGitare { get => _tipGitare; set => _tipGitare = value; }
        public DateTime DatumZaprimanja { get => _datumZaprimanja; set => _datumZaprimanja = value; }
        public long KupacId { get => _kupacId; set => _kupacId = value; }
    }
}
