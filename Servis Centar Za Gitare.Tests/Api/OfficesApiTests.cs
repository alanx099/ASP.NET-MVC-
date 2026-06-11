using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Servis_Centar_Za_Gitare.Data;
using Servis_Centar_Za_Gitare.Dtos;
using Servis_Centar_Za_Gitare.models;
using Servis_Centar_Za_Gitare.Tests.Infrastructure;
using Xunit;

namespace Servis_Centar_Za_Gitare.Tests.Api;

public class OfficesApiTests : ApiTestBase
{
    public OfficesApiTests(CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetAll_ReturnsOkAndSeededOffices()
    {
        await TestDataSeeder.SeedOfficesAsync(Factory);
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/offices");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var offices = await response.Content.ReadFromJsonAsync<List<OfficeDto>>();
        offices.Should().NotBeNull();
        offices.Should().ContainSingle(office =>
            office.Id == TestDataSeeder.ExistingOfficeId &&
            office.Ime == "Test poslovnica");
    }

    [Fact]
    public async Task GetById_WhenOfficeExists_ReturnsOkAndOffice()
    {
        await TestDataSeeder.SeedOfficesAsync(Factory);
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync($"/api/offices/{TestDataSeeder.ExistingOfficeId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var office = await response.Content.ReadFromJsonAsync<OfficeDto>();
        office.Should().NotBeNull();
        office!.Id.Should().Be(TestDataSeeder.ExistingOfficeId);
        office.Ime.Should().Be("Test poslovnica");
        office.Adresa.Should().Be("Test adresa 1");
    }

    [Fact]
    public async Task GetById_WhenOfficeDoesNotExist_ReturnsNotFound()
    {
        await TestDataSeeder.SeedOfficesAsync(Factory);
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync($"/api/offices/{TestDataSeeder.MissingOfficeId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WhenRequestIsValid_ReturnsCreatedAndPersistsOffice()
    {
        await TestDataSeeder.SeedOfficesAsync(Factory);
        var client = CreateAuthenticatedClient();
        var request = new OfficeRequestDto
        {
            Ime = "Nova poslovnica",
            Adresa = "Nova adresa 2"
        };

        var response = await client.PostAsJsonAsync("/api/offices", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        var createdOffice = await response.Content.ReadFromJsonAsync<OfficeDto>();
        createdOffice.Should().NotBeNull();
        createdOffice!.Ime.Should().Be(request.Ime);
        createdOffice.Adresa.Should().Be(request.Adresa);

        var savedOffice = await FindAsync<Poslovnica>(createdOffice.Id);
        savedOffice.Should().NotBeNull();
        savedOffice!.Ime.Should().Be(request.Ime);
        savedOffice.Adresa.Should().Be(request.Adresa);
    }

    [Fact]
    public async Task Create_WhenRequestIsInvalid_ReturnsBadRequest()
    {
        await TestDataSeeder.SeedOfficesAsync(Factory);
        var client = CreateAuthenticatedClient();
        var invalidRequest = new OfficeRequestDto
        {
            Ime = string.Empty,
            Adresa = string.Empty
        };

        var response = await client.PostAsJsonAsync("/api/offices", invalidRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WhenOfficeExists_ReturnsNoContentAndPersistsChanges()
    {
        await TestDataSeeder.SeedOfficesAsync(Factory);
        var client = CreateAuthenticatedClient();
        var request = new OfficeRequestDto
        {
            Ime = "Azurirana poslovnica",
            Adresa = "Azurirana adresa 3"
        };

        var response = await client.PutAsJsonAsync($"/api/offices/{TestDataSeeder.ExistingOfficeId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var updatedOffice = await FindAsync<Poslovnica>(TestDataSeeder.ExistingOfficeId);
        updatedOffice.Should().NotBeNull();
        updatedOffice!.Ime.Should().Be(request.Ime);
        updatedOffice.Adresa.Should().Be(request.Adresa);
    }

    [Fact]
    public async Task Update_WhenOfficeDoesNotExist_ReturnsNotFound()
    {
        await TestDataSeeder.SeedOfficesAsync(Factory);
        var client = CreateAuthenticatedClient();
        var request = new OfficeRequestDto
        {
            Ime = "Nepostojeca poslovnica",
            Adresa = "Nepostojeca adresa"
        };

        var response = await client.PutAsJsonAsync($"/api/offices/{TestDataSeeder.MissingOfficeId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WhenRequestIsInvalid_ReturnsBadRequest()
    {
        await TestDataSeeder.SeedOfficesAsync(Factory);
        var client = CreateAuthenticatedClient();
        var invalidRequest = new OfficeRequestDto
        {
            Ime = string.Empty,
            Adresa = string.Empty
        };

        var response = await client.PutAsJsonAsync($"/api/offices/{TestDataSeeder.ExistingOfficeId}", invalidRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_WhenOfficeExistsAndUserIsAdmin_ReturnsNoContentAndDeletesOffice()
    {
        await TestDataSeeder.SeedOfficesAsync(Factory);
        var client = CreateAuthenticatedClient(IdentitySeed.AdminRole);
        var createResponse = await client.PostAsJsonAsync("/api/offices", new OfficeRequestDto
        {
            Ime = "Poslovnica za brisanje",
            Adresa = "Adresa za brisanje"
        });
        var createdOffice = await createResponse.Content.ReadFromJsonAsync<OfficeDto>();

        var response = await client.DeleteAsync($"/api/offices/{createdOffice!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var deletedOffice = await FindAsync<Poslovnica>(createdOffice.Id);
        deletedOffice.Should().BeNull();
    }

    [Fact]
    public async Task Delete_WhenOfficeDoesNotExistAndUserIsAdmin_ReturnsNotFound()
    {
        await TestDataSeeder.SeedOfficesAsync(Factory);
        var client = CreateAuthenticatedClient(IdentitySeed.AdminRole);

        var response = await client.DeleteAsync($"/api/offices/{TestDataSeeder.MissingOfficeId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WhenUserIsManager_ReturnsForbidden()
    {
        await TestDataSeeder.SeedOfficesAsync(Factory);
        var client = CreateAuthenticatedClient(IdentitySeed.ManagerRole);

        var response = await client.DeleteAsync($"/api/offices/{TestDataSeeder.ExistingOfficeId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAll_WhenUserIsNotAuthenticated_ReturnsUnauthorized()
    {
        await TestDataSeeder.SeedOfficesAsync(Factory);
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/offices");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
