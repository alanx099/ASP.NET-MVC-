using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Servis_Centar_Za_Gitare.Data;
using Servis_Centar_Za_Gitare.models;

namespace Servis_Centar_Za_Gitare.Tests.Infrastructure;

public static class TestDataSeeder
{
    public const long ExistingOfficeId = 1;
    public const long ExistingCustomerId = 1;
    public const long ExistingEmployeeId = 1;
    public const long ExistingTechnicianId = 2;
    public const long ExistingGuitarId = 1;
    public const long ExistingRepairId = 1;
    public const int ExistingBrandId = 1;
    public const int ExistingGuitarTypeId = 1;
    public const int SecondGuitarTypeId = 2;
    public const int ExistingRepairStatusId = 1;
    public const int SecondRepairStatusId = 2;
    public const int ExistingRepairTypeId = 1;
    public const int SecondRepairTypeId = 2;
    public const long MissingOfficeId = 999;
    public const long MissingLongId = 999;
    public const int MissingIntId = 999;
    public const string TestUserId = "integration-test-user";

    public static async Task SeedOfficesAsync(CustomWebApplicationFactory factory)
    {
        await SeedApiDataAsync(factory);
    }

    public static async Task SeedApiDataAsync(CustomWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();

        db.Poslovnice.Add(new Poslovnica
        {
            Id = ExistingOfficeId,
            Ime = "Test poslovnica",
            Adresa = "Test adresa 1"
        });

        db.Users.Add(new AppUser
        {
            Id = TestUserId,
            UserName = "integration-test-user@example.com",
            NormalizedUserName = "INTEGRATION-TEST-USER@EXAMPLE.COM",
            Email = "integration-test-user@example.com",
            NormalizedEmail = "INTEGRATION-TEST-USER@EXAMPLE.COM",
            EmailConfirmed = true
        });

        db.Stranke.Add(new Stranka
        {
            Id = ExistingCustomerId,
            Ime = "Test",
            Prezime = "Kupac",
            Email = "test.kupac@example.com",
            BrojTelefona = "+385911111111",
            Adresa = "Kupac adresa 1",
            DatumRegistracije = "2026-06-10T10:00",
            Napomena = "Seed customer",
            PoslovnicaId = ExistingOfficeId,
            AppUserId = TestUserId
        });

        db.Zaposlenici.Add(new Zaposlenik
        {
            Id = ExistingEmployeeId,
            Ime = "Test",
            Prezime = "Zaposlenik",
            Email = "test.zaposlenik@example.com",
            BrojTelefona = "+385922222222",
            Adresa = "Zaposlenik adresa 1",
            DatumZaposlenja = "2026-06-10T09:00",
            Placa = 1200,
            PoslovnicaId = ExistingOfficeId
        });

        db.Tehnicari.Add(new ZapTehnicar
        {
            Id = ExistingTechnicianId,
            Ime = "Test",
            Prezime = "Tehnicar",
            Email = "test.tehnicar@example.com",
            BrojTelefona = "+385933333333",
            Adresa = "Tehnicar adresa 1",
            DatumZaposlenja = "2026-06-10T09:00",
            Placa = 1500,
            PoslovnicaId = ExistingOfficeId
        });

        db.Gitare.Add(new Gitara
        {
            Id = ExistingGuitarId,
            SerijskiBroj = "TEST-GUITAR-001",
            MarkaId = ExistingBrandId,
            BrojZica = "6",
            TipGitareId = ExistingGuitarTypeId,
            DatumZaprimanja = new DateTime(2026, 6, 10, 10, 0, 0, DateTimeKind.Utc),
            KupacId = ExistingCustomerId
        });

        db.Znanja.Add(new Znanje(ExistingTechnicianId, ExistingGuitarTypeId, ExistingRepairTypeId));

        db.Nalozi.Add(new Nalog
        {
            Id = ExistingRepairId,
            GitaraId = ExistingGuitarId,
            StrankaId = ExistingCustomerId,
            TehnicarId = ExistingTechnicianId,
            PoslovnicaId = ExistingOfficeId,
            OpisKvara = "Seed repair description",
            DatumOtvaranja = new DateTime(2026, 6, 10, 11, 0, 0, DateTimeKind.Utc),
            StatusNalogaId = ExistingRepairStatusId,
            VrstaPopravkaId = ExistingRepairTypeId
        });

        await db.SaveChangesAsync();
    }
}
