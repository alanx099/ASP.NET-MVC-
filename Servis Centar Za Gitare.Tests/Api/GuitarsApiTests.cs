using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Servis_Centar_Za_Gitare.Data;
using Servis_Centar_Za_Gitare.Dtos;
using Servis_Centar_Za_Gitare.models;
using Servis_Centar_Za_Gitare.Tests.Infrastructure;
using Xunit;

namespace Servis_Centar_Za_Gitare.Tests.Api;

public class GuitarsApiTests : ApiTestBase
{
    public GuitarsApiTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAll_ReturnsOkAndGuitars()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient().GetAsync("/api/guitars");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var guitars = await response.Content.ReadFromJsonAsync<List<GuitarDto>>();
        guitars.Should().ContainSingle(guitar => guitar.Id == TestDataSeeder.ExistingGuitarId);
    }

    [Fact]
    public async Task GetById_WhenGuitarExists_ReturnsOkAndGuitar()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient().GetAsync($"/api/guitars/{TestDataSeeder.ExistingGuitarId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var guitar = await response.Content.ReadFromJsonAsync<GuitarDto>();
        guitar!.SerijskiBroj.Should().Be("TEST-GUITAR-001");
    }

    [Fact]
    public async Task GetById_WhenGuitarDoesNotExist_ReturnsNotFound()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient().GetAsync($"/api/guitars/{TestDataSeeder.MissingLongId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WhenRequestIsValid_ReturnsCreatedAndPersistsGuitar()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var request = ValidRequest("NEW-GUITAR-001");

        var response = await CreateAuthenticatedClient().PostAsJsonAsync("/api/guitars", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<GuitarDto>();
        created!.SerijskiBroj.Should().Be(request.SerijskiBroj);
        (await FindAsync<Gitara>(created.Id))!.SerijskiBroj.Should().Be(request.SerijskiBroj);
    }

    [Fact]
    public async Task Create_WhenRequestIsInvalid_ReturnsBadRequest()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var request = ValidRequest("BAD-GUITAR-001");
        request.MarkaId = TestDataSeeder.MissingIntId;

        var response = await CreateAuthenticatedClient().PostAsJsonAsync("/api/guitars", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WhenGuitarExists_ReturnsNoContentAndPersistsChanges()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var request = ValidRequest("UPDATED-GUITAR-001");

        var response = await CreateAuthenticatedClient().PutAsJsonAsync($"/api/guitars/{TestDataSeeder.ExistingGuitarId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var saved = await FindAsync<Gitara>(TestDataSeeder.ExistingGuitarId);
        saved!.SerijskiBroj.Should().Be(request.SerijskiBroj);
    }

    [Fact]
    public async Task Update_WhenGuitarDoesNotExist_ReturnsNotFound()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient().PutAsJsonAsync($"/api/guitars/{TestDataSeeder.MissingLongId}", ValidRequest("MISSING-GUITAR-001"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WhenRequestIsInvalid_ReturnsBadRequest()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var request = ValidRequest("BAD-GUITAR-002");
        request.KupacId = TestDataSeeder.MissingLongId;

        var response = await CreateAuthenticatedClient().PutAsJsonAsync($"/api/guitars/{TestDataSeeder.ExistingGuitarId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_WhenGuitarExistsAndUserIsAdmin_ReturnsNoContent()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var client = CreateAuthenticatedClient(IdentitySeed.AdminRole);
        var createResponse = await client.PostAsJsonAsync("/api/guitars", ValidRequest("DELETE-GUITAR-001"));
        var created = await createResponse.Content.ReadFromJsonAsync<GuitarDto>();

        var response = await client.DeleteAsync($"/api/guitars/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await FindAsync<Gitara>(created.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Delete_WhenGuitarDoesNotExistAndUserIsAdmin_ReturnsNotFound()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient(IdentitySeed.AdminRole).DeleteAsync($"/api/guitars/{TestDataSeeder.MissingLongId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static GuitarRequestDto ValidRequest(string serial) => new()
    {
        SerijskiBroj = serial,
        MarkaId = TestDataSeeder.ExistingBrandId,
        BrojZica = "6",
        TipGitareId = TestDataSeeder.ExistingGuitarTypeId,
        DatumZaprimanja = new DateTime(2026, 6, 10, 10, 0, 0, DateTimeKind.Utc),
        KupacId = TestDataSeeder.ExistingCustomerId
    };
}
