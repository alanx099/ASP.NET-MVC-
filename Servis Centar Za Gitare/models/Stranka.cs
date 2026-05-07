using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Servis_Centar_Za_Gitare.models
{
    public class Stranka
    {
        private long _id;
        private long? _poslovnicaId;
        private String _ime = string.Empty;
        private String _prezime = string.Empty;
        private String _email = string.Empty;
        private String _brojTelefona = string.Empty;
        private String _adresa = string.Empty;
        private String _datumRegistracije = string.Empty;
        private String _napomena = string.Empty;
        private List<Gitara> _gitare = new List<Gitara>();
        private List<Nalog> _nalozi = new List<Nalog>();

        public Stranka()
        {
            _gitare = new List<Gitara>();
        }

    public Stranka(long id, string ime, string prezime, string email, string brojTelefona, string adresa, string datumRegistracije, string napomena, List<Gitara> gitare)
    {
        Id = id;
        Ime = ime;
        Prezime = prezime;
        Email = email;
        BrojTelefona = brojTelefona;
        Adresa = adresa;
        DatumRegistracije = datumRegistracije;
        Napomena = napomena;
        Gitare = gitare;
    }

        [Key]
        public long Id { get => _id; set => _id = value; }

        public long? PoslovnicaId { get => _poslovnicaId; set => _poslovnicaId = value; }

        [ForeignKey(nameof(PoslovnicaId))]
        public virtual Poslovnica? Poslovnica { get; set; }

        [Required]
        [MaxLength(80)]
        public string Ime { get => _ime; set => _ime = value; }

        [Required]
        [MaxLength(80)]
        public string Prezime { get => _prezime; set => _prezime = value; }

        [Required]
        [MaxLength(120)]
        public string Email { get => _email; set => _email = value; }

        [Required]
        [MaxLength(30)]
        public string BrojTelefona { get => _brojTelefona; set => _brojTelefona = value; }

        [Required]
        [MaxLength(200)]
        public string Adresa { get => _adresa; set => _adresa = value; }

        [Required]
        [MaxLength(30)]
        public string DatumRegistracije { get => _datumRegistracije; set => _datumRegistracije = value; }

        [MaxLength(1000)]
        public string Napomena { get => _napomena; set => _napomena = value; }

        public virtual ICollection<Gitara> Gitare { get => _gitare; set => _gitare = value.ToList(); }
        public virtual ICollection<Nalog> Nalozi { get => _nalozi; set => _nalozi = value.ToList(); }
    }
}
