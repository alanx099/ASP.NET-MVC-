using System.ComponentModel.DataAnnotations;
using Servis_Centar_Za_Gitare.models;

namespace Servis_Centar_Za_Gitare.Dtos
{
    public record LookupDto(int Id, string Naziv);

    public record OfficeSummaryDto(long Id, string Ime, string Adresa);

    public record PersonSummaryDto(long Id, string Ime, string Prezime, string Email);

    public record CustomerSummaryDto(long Id, string Ime, string Prezime, string Email, string BrojTelefona);

    public record GuitarSummaryDto(long Id, string SerijskiBroj, LookupDto? Marka, LookupDto? TipGitare);

    public record TechnicianSummaryDto(long Id, string Ime, string Prezime, string Email);

    public record CustomerDto(
        long Id,
        string Ime,
        string Prezime,
        string Email,
        string BrojTelefona,
        string Adresa,
        string DatumRegistracije,
        string Napomena,
        OfficeSummaryDto? Poslovnica,
        IEnumerable<GuitarSummaryDto> Gitare);

    public class CustomerRequestDto
    {
        [Required, StringLength(80)]
        public string Ime { get; set; } = string.Empty;

        [Required, StringLength(80)]
        public string Prezime { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(120)]
        public string Email { get; set; } = string.Empty;

        [Required, Phone, StringLength(30)]
        public string BrojTelefona { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string Adresa { get; set; } = string.Empty;

        [Required, StringLength(30)]
        public string DatumRegistracije { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Napomena { get; set; } = string.Empty;

        public long? PoslovnicaId { get; set; }
    }

    public record GuitarDto(
        long Id,
        string SerijskiBroj,
        string BrojZica,
        DateTime DatumZaprimanja,
        LookupDto? Marka,
        LookupDto? TipGitare,
        CustomerSummaryDto? Kupac);

    public class GuitarRequestDto
    {
        [Required, StringLength(64)]
        public string SerijskiBroj { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int MarkaId { get; set; }

        [Required, RegularExpression(@"^\d{1,2}$"), StringLength(4)]
        public string BrojZica { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int TipGitareId { get; set; }

        public DateTime DatumZaprimanja { get; set; }

        [Range(1, long.MaxValue)]
        public long KupacId { get; set; }
    }

    public record RepairDto(
        long Id,
        string OpisKvara,
        DateTime DatumOtvaranja,
        DateTime? DatumZatvaranja,
        GuitarSummaryDto? Gitara,
        CustomerSummaryDto? Stranka,
        TechnicianSummaryDto? Tehnicar,
        OfficeSummaryDto? Poslovnica,
        LookupDto? StatusNaloga,
        LookupDto? VrstaPopravka);

    public class RepairRequestDto
    {
        [Range(1, long.MaxValue)]
        public long GitaraId { get; set; }

        [Range(1, long.MaxValue)]
        public long StrankaId { get; set; }

        public long? TehnicarId { get; set; }

        public long? PoslovnicaId { get; set; }

        [Required, StringLength(1000, MinimumLength = 3)]
        public string OpisKvara { get; set; } = string.Empty;

        public DateTime DatumOtvaranja { get; set; }

        public DateTime? DatumZatvaranja { get; set; }

        [Range(1, int.MaxValue)]
        public int StatusNalogaId { get; set; }

        [Range(1, int.MaxValue)]
        public int VrstaPopravkaId { get; set; }
    }

    public record OfficeDto(
        long Id,
        string Ime,
        string Adresa,
        int BrojStranaka,
        int BrojZaposlenika,
        int BrojNaloga);

    public class OfficeRequestDto
    {
        [Required, MaxLength(120)]
        public string Ime { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Adresa { get; set; } = string.Empty;
    }

    public record EmployeeDto(
        long Id,
        string Ime,
        string Prezime,
        string Email,
        string BrojTelefona,
        string Adresa,
        string DatumZaposlenja,
        double Placa,
        OfficeSummaryDto? Poslovnica);

    public class EmployeeRequestDto
    {
        [Required, StringLength(80)]
        public string Ime { get; set; } = string.Empty;

        [Required, StringLength(80)]
        public string Prezime { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(120)]
        public string Email { get; set; } = string.Empty;

        [Required, Phone, StringLength(30)]
        public string BrojTelefona { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string Adresa { get; set; } = string.Empty;

        [Required, StringLength(30)]
        public string DatumZaposlenja { get; set; } = string.Empty;

        [Range(0, 1000000)]
        public double Placa { get; set; }

        public long? PoslovnicaId { get; set; }
    }

    public record KnowledgeDto(long TehnicarId, LookupDto? TipGitare, LookupDto? VrstaPopravka);

    public record TechnicianDto(
        long Id,
        string Ime,
        string Prezime,
        string Email,
        string BrojTelefona,
        string Adresa,
        string DatumZaposlenja,
        double Placa,
        OfficeSummaryDto? Poslovnica,
        IEnumerable<KnowledgeDto> Znanja);

    public class TechnicianRequestDto : EmployeeRequestDto
    {
        public IEnumerable<KnowledgeRequestDto> Znanja { get; set; } = Array.Empty<KnowledgeRequestDto>();
    }

    public class KnowledgeRequestDto
    {
        [Range(1, long.MaxValue)]
        public long TehnicarId { get; set; }

        [Range(1, int.MaxValue)]
        public int TipGitareId { get; set; }

        [Range(1, int.MaxValue)]
        public int VrstaPopravkaId { get; set; }
    }

    public class LookupRequestDto
    {
        [Required, MaxLength(80)]
        public string Naziv { get; set; } = string.Empty;
    }

    public static class ApiDtoMapper
    {
        public static LookupDto? ToDto(Marka? item) => item == null ? null : new LookupDto(item.Id, item.Naziv);
        public static LookupDto? ToDto(TipGitare? item) => item == null ? null : new LookupDto(item.Id, item.Naziv);
        public static LookupDto? ToDto(StatusNaloga? item) => item == null ? null : new LookupDto(item.Id, item.Naziv);
        public static LookupDto? ToDto(VrstaPopravka? item) => item == null ? null : new LookupDto(item.Id, item.Naziv);

        public static OfficeSummaryDto? ToSummaryDto(Poslovnica? item)
        {
            return item == null ? null : new OfficeSummaryDto(item.Id, item.Ime, item.Adresa);
        }

        public static CustomerSummaryDto? ToSummaryDto(Stranka? item)
        {
            return item == null ? null : new CustomerSummaryDto(item.Id, item.Ime, item.Prezime, item.Email, item.BrojTelefona);
        }

        public static GuitarSummaryDto? ToSummaryDto(Gitara? item)
        {
            return item == null ? null : new GuitarSummaryDto(item.Id, item.SerijskiBroj, ToDto(item.Marka), ToDto(item.TipGitare));
        }

        public static TechnicianSummaryDto? ToSummaryDto(ZapTehnicar? item)
        {
            return item == null ? null : new TechnicianSummaryDto(item.Id, item.Ime, item.Prezime, item.Email);
        }

        public static CustomerDto ToDto(Stranka item)
        {
            return new CustomerDto(
                item.Id,
                item.Ime,
                item.Prezime,
                item.Email,
                item.BrojTelefona,
                item.Adresa,
                item.DatumRegistracije,
                item.Napomena,
                ToSummaryDto(item.Poslovnica),
                item.Gitare.Select(ToSummaryDto).Where(guitar => guitar != null)!);
        }

        public static GuitarDto ToDto(Gitara item)
        {
            return new GuitarDto(
                item.Id,
                item.SerijskiBroj,
                item.BrojZica,
                item.DatumZaprimanja,
                ToDto(item.Marka),
                ToDto(item.TipGitare),
                ToSummaryDto(item.Kupac));
        }

        public static RepairDto ToDto(Nalog item)
        {
            return new RepairDto(
                item.Id,
                item.OpisKvara,
                item.DatumOtvaranja,
                item.DatumZatvaranja == default ? null : item.DatumZatvaranja,
                ToSummaryDto(item.Gitara),
                ToSummaryDto(item.Stranka),
                ToSummaryDto(item.Tehnicar),
                ToSummaryDto(item.Poslovnica),
                ToDto(item.StatusNaloga),
                ToDto(item.VrstaPopravka));
        }

        public static OfficeDto ToDto(Poslovnica item)
        {
            return new OfficeDto(item.Id, item.Ime, item.Adresa, item.Stranke.Count, item.Zaposlenici.Count, item.Nalozi.Count);
        }

        public static EmployeeDto ToDto(Zaposlenik item)
        {
            return new EmployeeDto(
                item.Id,
                item.Ime,
                item.Prezime,
                item.Email,
                item.BrojTelefona,
                item.Adresa,
                item.DatumZaposlenja,
                item.Placa,
                ToSummaryDto(item.Poslovnica));
        }

        public static TechnicianDto ToDto(ZapTehnicar item)
        {
            return new TechnicianDto(
                item.Id,
                item.Ime,
                item.Prezime,
                item.Email,
                item.BrojTelefona,
                item.Adresa,
                item.DatumZaposlenja,
                item.Placa,
                ToSummaryDto(item.Poslovnica),
                item.Znanja.Select(ToDto));
        }

        public static KnowledgeDto ToDto(Znanje item)
        {
            return new KnowledgeDto(item.TehnicarId, ToDto(item.TipGitare), ToDto(item.VrstaPopravka));
        }
    }
}
