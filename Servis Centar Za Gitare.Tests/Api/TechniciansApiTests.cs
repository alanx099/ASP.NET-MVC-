using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Servis_Centar_Za_Gitare.Data;
using Servis_Centar_Za_Gitare.Dtos;
using Servis_Centar_Za_Gitare.models;
using Servis_Centar_Za_Gitare.Tests.Infrastructure;
using Xunit;

namespace Servis_Centar_Za_Gitare.Tests.Api;

public class TechniciansApiTests : ApiTestBase
{
    public TechniciansApiTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAll_ReturnsOkAndTechnicians()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient().GetAsync("/api/technicians");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var technicians = await response.Content.ReadFromJsonAsync<List<TechnicianDto>>();
        technicians.Should().ContainSingle(technician => technician.Id == TestDataSeeder.ExistingTechnicianId);
    }

    [Fact]
    public async Task GetById_WhenTechnicianExists_ReturnsOkAndTechnician()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient().GetAsync($"/api/technicians/{TestDataSeeder.ExistingTechnicianId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var technician = await response.Content.ReadFromJsonAsync<TechnicianDto>();
        technician!.Email.Should().Be("test.tehnicar@example.com");
    }

    [Fact]
    public async Task GetById_WhenTechnicianDoesNotExist_ReturnsNotFound()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient().GetAsync($"/api/technicians/{TestDataSeeder.MissingLongId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WhenRequestIsValid_ReturnsCreatedAndPersistsTechnician()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var request = ValidRequest("new.technician@example.com");

        var response = await CreateAuthenticatedClient().PostAsJsonAsync("/api/technicians", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<TechnicianDto>();
        created!.Email.Should().Be(request.Email);
        (await FindAsync<ZapTehnicar>(created.Id)).Should().NotBeNull();
    }

    [Fact]
    public async Task Create_WhenRequestIsInvalid_ReturnsBadRequest()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var request = ValidRequest("invalid.technician@example.com");
        request.Znanja = Array.Empty<KnowledgeRequestDto>();

        var response = await CreateAuthenticatedClient().PostAsJsonAsync("/api/technicians", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WhenTechnicianExists_ReturnsNoContentAndPersistsChanges()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var request = ValidRequest("updated.technician@example.com");
        request.Ime = "Updated";

        var response = await CreateAuthenticatedClient().PutAsJsonAsync($"/api/technicians/{TestDataSeeder.ExistingTechnicianId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var saved = await FindAsync<ZapTehnicar>(TestDataSeeder.ExistingTechnicianId);
        saved!.Ime.Should().Be("Updated");
    }

    [Fact]
    public async Task Update_WhenTechnicianDoesNotExist_ReturnsNotFound()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient().PutAsJsonAsync($"/api/technicians/{TestDataSeeder.MissingLongId}", ValidRequest("missing.technician@example.com"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WhenRequestIsInvalid_ReturnsBadRequest()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var request = ValidRequest("bad.technician@example.com");
        request.PoslovnicaId = TestDataSeeder.MissingLongId;

        var response = await CreateAuthenticatedClient().PutAsJsonAsync($"/api/technicians/{TestDataSeeder.ExistingTechnicianId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_WhenTechnicianExistsAndUserIsAdmin_ReturnsNoContent()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var client = CreateAuthenticatedClient(IdentitySeed.AdminRole);
        var createResponse = await client.PostAsJsonAsync("/api/technicians", ValidRequest("delete.technician@example.com"));
        var created = await createResponse.Content.ReadFromJsonAsync<TechnicianDto>();

        var response = await client.DeleteAsync($"/api/technicians/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await FindAsync<ZapTehnicar>(created.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Delete_WhenTechnicianDoesNotExistAndUserIsAdmin_ReturnsNotFound()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient(IdentitySeed.AdminRole).DeleteAsync($"/api/technicians/{TestDataSeeder.MissingLongId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static TechnicianRequestDto ValidRequest(string email) => new()
    {
        Ime = "Novi",
        Prezime = "Tehnicar",
        Email = email,
        BrojTelefona = "+385931234567",
        Adresa = "Nova adresa tehnicara",
        DatumZaposlenja = "2026-06-10T09:00",
        Placa = 1600,
        PoslovnicaId = TestDataSeeder.ExistingOfficeId,
        Znanja = new[]
        {
            new KnowledgeRequestDto
            {
                TehnicarId = TestDataSeeder.ExistingTechnicianId,
                TipGitareId = TestDataSeeder.ExistingGuitarTypeId,
                VrstaPopravkaId = TestDataSeeder.ExistingRepairTypeId
            }
        }
    };
}
