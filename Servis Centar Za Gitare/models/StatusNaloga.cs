using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Servis_Centar_Za_Gitare.models
{
    public class StatusNaloga
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(80)]
        public string Naziv { get; set; } = string.Empty;

        public virtual ICollection<Nalog> Nalozi { get; set; } = new List<Nalog>();
    }
}
