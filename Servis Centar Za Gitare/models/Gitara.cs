using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Servis_Centar_Za_Gitare.models
{
    public class Gitara
    {
        private long _id;
        private String _serijskiBroj = string.Empty;
        private int _markaId;
        private String _brojZica = string.Empty;
        private int _tipGitareId;
        private DateTime _datumZaprimanja;
        private long _kupacId;

        public Gitara()
        {
        }

        public Gitara(long id, string serijskiBroj, int markaId, string brojZica, int tipGitareId, DateTime datumZaprimanja, long kupacId)
        {
            Id = id;
            SerijskiBroj = serijskiBroj;
            MarkaId = markaId;
            BrojZica = brojZica;
            TipGitareId = tipGitareId;
            DatumZaprimanja = datumZaprimanja;
            KupacId = kupacId;
        }

        [Key]
        public long Id { get => _id; set => _id = value; }

        [Required]
        [MaxLength(64)]
        public string SerijskiBroj { get => _serijskiBroj; set => _serijskiBroj = value; }

        [Required]
        public int MarkaId { get => _markaId; set => _markaId = value; }

        [ForeignKey(nameof(MarkaId))]
        public virtual Marka Marka { get; set; } = null!;

        [Required]
        [MaxLength(4)]
        public string BrojZica { get => _brojZica; set => _brojZica = value; }

        [Required]
        public int TipGitareId { get => _tipGitareId; set => _tipGitareId = value; }

        [ForeignKey(nameof(TipGitareId))]
        public virtual TipGitare TipGitare { get; set; } = null!;

        public DateTime DatumZaprimanja { get => _datumZaprimanja; set => _datumZaprimanja = value; }

        [Required]
        public long KupacId { get => _kupacId; set => _kupacId = value; }

        [ForeignKey(nameof(KupacId))]
        public virtual Stranka Kupac { get; set; } = null!;

        public virtual ICollection<Nalog> Nalozi { get; set; } = new List<Nalog>();
    }
}
