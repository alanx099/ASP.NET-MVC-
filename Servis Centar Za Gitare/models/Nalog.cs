using System;
using System.Collections.Generic;
using System.Text;
using Servis_Centar_Za_Gitare.enums;

namespace Servis_Centar_Za_Gitare.models
{
    public class Nalog
    {
        private Gitara _gitara;
        private Stranka _stranka;
        private ZapTehnicar _tehnicar;
        private String _opisKvara;
        private DateTime _datumOtvaranja;
        private DateTime _datumZatvaranja;
        private StatusNalogaEnum _status;
        private VrstaPopravkaEnum _vrstaPopravka;

        public Gitara Gitara
        {
            get { return _gitara; }
            set { _gitara = value; }
        }

        public Stranka Stranka
        {
            get { return _stranka; }
            set { _stranka = value; }
        }

        public ZapTehnicar Tehnicar
        {
            get { return _tehnicar; }
            set { _tehnicar = value; }
        }

        public String OpisKvara
        {
            get { return _opisKvara; }
            set { _opisKvara = value; }
        }

        public DateTime DatumOtvaranja
        {
            get { return _datumOtvaranja; }
            set { _datumOtvaranja = value; }
        }

        public DateTime DatumZatvaranja
        {
            get { return _datumZatvaranja; }
            set { _datumZatvaranja = value; }
        }

        public StatusNalogaEnum Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public VrstaPopravkaEnum VrstaPopravka
        {
            get { return _vrstaPopravka; }
            set { _vrstaPopravka = value; }
        }

        public Nalog() { }

        public Nalog(Gitara gitara, Stranka stranka, ZapTehnicar tehnicar, String opisKvara,
            DateTime datumOtvaranja, DateTime datumZatvaranja, StatusNalogaEnum status, VrstaPopravkaEnum vrstaPopravka)
        {
            Gitara = gitara;
            Stranka = stranka;
            Tehnicar = tehnicar;
            OpisKvara = opisKvara;
            DatumOtvaranja = datumOtvaranja;
            DatumZatvaranja = datumZatvaranja;
            Status = status;
            VrstaPopravka = vrstaPopravka;
        }
    }
}
