using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Servis_Centar_Za_Gitare.models
{
    public class Znanje
    {
        private long _tehnicarId;
        private int _tipGitareId;
        private int _vrstaPopravkaId;

        [Required]
        public long TehnicarId
        {
            get { return _tehnicarId; }
            set { _tehnicarId = value; }
        }

        [ForeignKey(nameof(TehnicarId))]
        public virtual ZapTehnicar Tehnicar { get; set; } = null!;

        [Required]
        public int TipGitareId
        {
            get { return _tipGitareId; }
            set { _tipGitareId = value; }
        }

        [ForeignKey(nameof(TipGitareId))]
        public virtual TipGitare TipGitare { get; set; } = null!;

        [Required]
        public int VrstaPopravkaId
        {
            get { return _vrstaPopravkaId; }
            set { _vrstaPopravkaId = value; }
        }

        [ForeignKey(nameof(VrstaPopravkaId))]
        public virtual VrstaPopravka VrstaPopravka { get; set; } = null!;

        public Znanje() { }

        public Znanje(int tipGitareId, int vrstaPopravkaId)
        {
            TipGitareId = tipGitareId;
            VrstaPopravkaId = vrstaPopravkaId;
        }

        public Znanje(long tehnicarId, int tipGitareId, int vrstaPopravkaId)
        {
            TehnicarId = tehnicarId;
            TipGitareId = tipGitareId;
            VrstaPopravkaId = vrstaPopravkaId;
        }
    }
}
