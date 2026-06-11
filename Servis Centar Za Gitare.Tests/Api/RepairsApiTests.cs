using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Servis_Centar_Za_Gitare.Data;
using Servis_Centar_Za_Gitare.Dtos;
using Servis_Centar_Za_Gitare.models;
using Servis_Centar_Za_Gitare.Tests.Infrastructure;
using Xunit;

namespace Servis_Centar_Za_Gitare.Tests.Api;

public class RepairsApiTests : ApiTestBase
{
    public RepairsApiTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAll_ReturnsOkAndRepairs()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient().GetAsync("/api/repairs");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var repairs = await response.Content.ReadFromJsonAsync<List<RepairDto>>();
        repairs.Should().ContainSingle(repair => repair.Id == TestDataSeeder.ExistingRepairId);
    }

    [Fact]
    public async Task GetById_WhenRepairExists_ReturnsOkAndRepair()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient().GetAsync($"/api/repairs/{TestDataSeeder.ExistingRepairId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var repair = await response.Content.ReadFromJsonAsync<RepairDto>();
        repair!.OpisKvara.Should().Be("Seed repair description");
    }

    [Fact]
    public async Task GetById_WhenRepairDoesNotExist_ReturnsNotFound()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient().GetAsync($"/api/repairs/{TestDataSeeder.MissingLongId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WhenRequestIsValid_ReturnsCreatedAndPersistsRepair()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var request = ValidRequest("New repair description");

        var response = await CreateAuthenticatedClient().PostAsJsonAsync("/api/repairs", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<RepairDto>();
        created!.OpisKvara.Should().Be(request.OpisKvara);
        (await FindAsync<Nalog>(created.Id)).Should().NotBeNull();
    }

    [Fact]
    public async Task Create_WhenRequestIsInvalid_ReturnsBadRequest()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var request = ValidRequest("Bad repair description");
        request.GitaraId = TestDataSeeder.MissingLongId;

        var response = await CreateAuthenticatedClient().PostAsJsonAsync("/api/repairs", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WhenRepairExists_ReturnsNoContentAndPersistsChanges()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var request = ValidRequest("Updated repair description");

        var response = await CreateAuthenticatedClient().PutAsJsonAsync($"/api/repairs/{TestDataSeeder.ExistingRepairId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var saved = await FindAsync<Nalog>(TestDataSeeder.ExistingRepairId);
        saved!.OpisKvara.Should().Be(request.OpisKvara);
    }

    [Fact]
    public async Task Update_WhenRepairDoesNotExist_ReturnsNotFound()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient().PutAsJsonAsync($"/api/repairs/{TestDataSeeder.MissingLongId}", ValidRequest("Missing repair description"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WhenRequestIsInvalid_ReturnsBadRequest()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var request = ValidRequest("Bad repair description");
        request.StatusNalogaId = TestDataSeeder.MissingIntId;

        var response = await CreateAuthenticatedClient().PutAsJsonAsync($"/api/repairs/{TestDataSeeder.ExistingRepairId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_WhenRepairExistsAndUserIsAdmin_ReturnsNoContent()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient(IdentitySeed.AdminRole).DeleteAsync($"/api/repairs/{TestDataSeeder.ExistingRepairId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await FindAsync<Nalog>(TestDataSeeder.ExistingRepairId)).Should().BeNull();
    }

    [Fact]
    public async Task Delete_WhenRepairDoesNotExistAndUserIsAdmin_ReturnsNotFound()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient(IdentitySeed.AdminRole).DeleteAsync($"/api/repairs/{TestDataSeeder.MissingLongId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static RepairRequestDto ValidRequest(string description) => new()
    {
        GitaraId = TestDataSeeder.ExistingGuitarId,
        StrankaId = TestDataSeeder.ExistingCustomerId,
        TehnicarId = TestDataSeeder.ExistingTechnicianId,
        PoslovnicaId = TestDataSeeder.ExistingOfficeId,
        OpisKvara = description,
        DatumOtvaranja = new DateTime(2026, 6, 10, 11, 0, 0, DateTimeKind.Utc),
        DatumZatvaranja = null,
        StatusNalogaId = TestDataSeeder.ExistingRepairStatusId,
        VrstaPopravkaId = TestDataSeeder.ExistingRepairTypeId
    };
}
