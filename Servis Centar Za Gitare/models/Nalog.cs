using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Servis_Centar_Za_Gitare.models
{
    public class Nalog
    {
        private long _id;
        private long _gitaraId;
        private long _strankaId;
        private long _tehnicarId;
        private long? _poslovnicaId;
        private Gitara _gitara = null!;
        private Stranka _stranka = null!;
        private ZapTehnicar _tehnicar = null!;
        private String _opisKvara = string.Empty;
        private DateTime _datumOtvaranja;
        private DateTime _datumZatvaranja;
        private int _statusNalogaId;
        private int _vrstaPopravkaId;

        [Key]
        public long Id
        {
            get { return _id; }
            set { _id = value; }
        }

        [Required]
        public long GitaraId
        {
            get { return _gitaraId; }
            set { _gitaraId = value; }
        }

        [Required]
        public long StrankaId
        {
            get { return _strankaId; }
            set { _strankaId = value; }
        }

        [Required]
        public long TehnicarId
        {
            get { return _tehnicarId; }
            set { _tehnicarId = value; }
        }

        public long? PoslovnicaId
        {
            get { return _poslovnicaId; }
            set { _poslovnicaId = value; }
        }

        [ForeignKey(nameof(GitaraId))]
        [ValidateNever]
        public virtual Gitara Gitara
        {
            get { return _gitara; }
            set
            {
                _gitara = value;
                _gitaraId = value.Id;
            }
        }

        [ForeignKey(nameof(StrankaId))]
        [ValidateNever]
        public virtual Stranka Stranka
        {
            get { return _stranka; }
            set
            {
                _stranka = value;
                _strankaId = value.Id;
            }
        }

        [ForeignKey(nameof(TehnicarId))]
        [ValidateNever]
        public virtual ZapTehnicar Tehnicar
        {
            get { return _tehnicar; }
            set
            {
                _tehnicar = value;
                _tehnicarId = value.Id;
            }
        }

        [ForeignKey(nameof(PoslovnicaId))]
        [ValidateNever]
        public virtual Poslovnica? Poslovnica { get; set; }

        [Required]
        [MaxLength(1000)]
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

        [Required]
        public int StatusNalogaId
        {
            get { return _statusNalogaId; }
            set { _statusNalogaId = value; }
        }

        [ForeignKey(nameof(StatusNalogaId))]
        [ValidateNever]
        public virtual StatusNaloga StatusNaloga { get; set; } = null!;

        [Required]
        public int VrstaPopravkaId
        {
            get { return _vrstaPopravkaId; }
            set { _vrstaPopravkaId = value; }
        }

        [ForeignKey(nameof(VrstaPopravkaId))]
        [ValidateNever]
        public virtual VrstaPopravka VrstaPopravka { get; set; } = null!;

        public Nalog() { }

        public Nalog(Gitara gitara, Stranka stranka, ZapTehnicar tehnicar, String opisKvara,
            DateTime datumOtvaranja, DateTime datumZatvaranja, int statusNalogaId, int vrstaPopravkaId)
        {
            Gitara = gitara;
            Stranka = stranka;
            Tehnicar = tehnicar;
            PoslovnicaId = tehnicar.PoslovnicaId;
            OpisKvara = opisKvara;
            DatumOtvaranja = datumOtvaranja;
            DatumZatvaranja = datumZatvaranja;
            StatusNalogaId = statusNalogaId;
            VrstaPopravkaId = vrstaPopravkaId;
        }
    }
}
