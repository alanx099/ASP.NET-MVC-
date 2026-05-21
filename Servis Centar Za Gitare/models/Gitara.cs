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

        [Required(ErrorMessage = "Serial number is required.")]
        [StringLength(64, ErrorMessage = "Serial number can contain up to 64 characters.")]
        public string SerijskiBroj { get => _serijskiBroj; set => _serijskiBroj = value; }

        [Range(1, int.MaxValue, ErrorMessage = "Brand is required.")]
        public int MarkaId { get => _markaId; set => _markaId = value; }

        [ForeignKey(nameof(MarkaId))]
        public virtual Marka Marka { get; set; } = null!;

        [Required(ErrorMessage = "Number of strings is required.")]
        [RegularExpression(@"^\d{1,2}$", ErrorMessage = "Enter a valid number of strings.")]
        [StringLength(4, ErrorMessage = "Number of strings can contain up to 4 characters.")]
        public string BrojZica { get => _brojZica; set => _brojZica = value; }

        [Range(1, int.MaxValue, ErrorMessage = "Guitar type is required.")]
        public int TipGitareId { get => _tipGitareId; set => _tipGitareId = value; }

        [ForeignKey(nameof(TipGitareId))]
        public virtual TipGitare TipGitare { get; set; } = null!;

        public DateTime DatumZaprimanja { get => _datumZaprimanja; set => _datumZaprimanja = value; }

        [Range(1, long.MaxValue, ErrorMessage = "Owner is required.")]
        public long KupacId { get => _kupacId; set => _kupacId = value; }

        [ForeignKey(nameof(KupacId))]
        public virtual Stranka Kupac { get; set; } = null!;

        public virtual ICollection<Nalog> Nalozi { get; set; } = new List<Nalog>();
    }
}
