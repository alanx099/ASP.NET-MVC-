using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Servis_Centar_Za_Gitare.models
{
    public class TipGitare
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(80)]
        public string Naziv { get; set; } = string.Empty;

        public virtual ICollection<Gitara> Gitare { get; set; } = new List<Gitara>();
        public virtual ICollection<Znanje> Znanja { get; set; } = new List<Znanje>();
    }
}
