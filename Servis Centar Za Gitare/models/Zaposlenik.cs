using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Servis_Centar_Za_Gitare.models
{
    public class Zaposlenik
    {
        private long _id;
        private long? _poslovnicaId;
        private String _ime = string.Empty;
        private String _prezime = string.Empty;
        private String _email = string.Empty;
        private String _brojTelefona = string.Empty;
        private String _adresa = string.Empty;
        private String _datumZaposlenja = string.Empty;
        private double _placa;
        private List<Nalog> _nalozi = new List<Nalog>();

        public Zaposlenik()
        {
        }

        public Zaposlenik(long id, string ime, string prezime, string email, string brojTelefona, string adresa, string datumZaposlenja, double placa)
        {
            Id = id;
            Ime = ime;
            Prezime = prezime;
            Email = email;
            BrojTelefona = brojTelefona;
            Adresa = adresa;
            DatumZaposlenja = datumZaposlenja;
            Placa = placa; 
        }

        [Key]
        public long Id { get => _id; set => _id = value; }

        public long? PoslovnicaId { get => _poslovnicaId; set => _poslovnicaId = value; }

        [ForeignKey(nameof(PoslovnicaId))]
        public virtual Poslovnica? Poslovnica { get; set; }

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

        [Required(ErrorMessage = "Hire date is required.")]
        [StringLength(30, ErrorMessage = "Hire date can contain up to 30 characters.")]
        public string DatumZaposlenja { get => _datumZaposlenja; set => _datumZaposlenja = value; }

        [Range(0, 1000000, ErrorMessage = "Salary must be between 0 and 1,000,000.")]
        public double Placa { get => _placa; set => _placa = value; }

        public virtual ICollection<Nalog> Nalozi { get => _nalozi; set => _nalozi = value.ToList(); }
    }
}
