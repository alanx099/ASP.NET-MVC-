using System;
using System.Collections.Generic;
using System.Text;
using Servis_Centar_Za_Gitare.enums;

namespace Servis_Centar_Za_Gitare.models
{
    public class Gitara
    {
        private long _id;
        private String _serijskiBroj = string.Empty;
        private MarkeEnum _marka;
        private String _brojZica = string.Empty;
        private TipGitareEnum _tipGitare;
        private DateTime _datumZaprimanja;
        private long _kupacId;

        public Gitara()
        {
        }

        public Gitara(long id, string serijskiBroj, MarkeEnum marka, string brojZica, TipGitareEnum tipGitare, DateTime datumZaprimanja, long kupacId)
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
        public MarkeEnum Marka { get => _marka; set => _marka = value; }
        public string BrojZica { get => _brojZica; set => _brojZica = value; }
        public TipGitareEnum TipGitare { get => _tipGitare; set => _tipGitare = value; }
        public DateTime DatumZaprimanja { get => _datumZaprimanja; set => _datumZaprimanja = value; }
        public long KupacId { get => _kupacId; set => _kupacId = value; }
    }
}
