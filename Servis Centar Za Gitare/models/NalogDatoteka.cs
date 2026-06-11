using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Servis_Centar_Za_Gitare.models
{
    public class NalogDatoteka
    {
        [Key]
        public long Id { get; set; }

        [Range(1, long.MaxValue)]
        public long NalogId { get; set; }

        [ForeignKey(nameof(NalogId))]
        public virtual Nalog Nalog { get; set; } = null!;

        [Required]
        [MaxLength(255)]
        public string OriginalniNaziv { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string SpremljeniNaziv { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string RelativnaPutanja { get; set; } = string.Empty;

        [Required]
        [MaxLength(120)]
        public string TipSadrzaja { get; set; } = string.Empty;

        public long VelicinaBajtova { get; set; }

        public DateTime DatumUploada { get; set; }
    }
}
