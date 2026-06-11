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
        private string? _appUserId;
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

        public string? AppUserId { get => _appUserId; set => _appUserId = value; }

        [ForeignKey(nameof(AppUserId))]
        public virtual AppUser? AppUser { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [StringLength(80, ErrorMessage = "First name can contain up to 80 characters.")]
        public string Ime { get => _ime; set => _ime = value; }

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(80, ErrorMessage = "Last name can contain up to 80 characters.")]
        public string Prezime { get => _prezime; set => _prezime = value; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(120, ErrorMessage = "Email can contain up to 120 characters.")]
        public string Email { get => _email; set => _email = value; }

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Enter a valid phone number.")]
        [StringLength(30, ErrorMessage = "Phone number can contain up to 30 characters.")]
        public string BrojTelefona { get => _brojTelefona; set => _brojTelefona = value; }

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(200, ErrorMessage = "Address can contain up to 200 characters.")]
        public string Adresa { get => _adresa; set => _adresa = value; }

        [Required(ErrorMessage = "Registration date is required.")]
        [StringLength(30, ErrorMessage = "Registration date can contain up to 30 characters.")]
        public string DatumRegistracije { get => _datumRegistracije; set => _datumRegistracije = value; }

        [StringLength(1000, ErrorMessage = "Notes can contain up to 1000 characters.")]
        public string Napomena { get => _napomena; set => _napomena = value; }

        public virtual ICollection<Gitara> Gitare { get => _gitare; set => _gitare = value.ToList(); }
        public virtual ICollection<Nalog> Nalozi { get => _nalozi; set => _nalozi = value.ToList(); }
    }
}
