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
        public string DatumZaposlenja { get => _datumZaposlenja; set => _datumZaposlenja = value; }

        public double Placa { get => _placa; set => _placa = value; }

        public virtual ICollection<Nalog> Nalozi { get => _nalozi; set => _nalozi = value.ToList(); }
    }
}
