using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Servis_Centar_Za_Gitare.Data;
using Servis_Centar_Za_Gitare.Dtos;
using Servis_Centar_Za_Gitare.Tests.Infrastructure;
using Xunit;

namespace Servis_Centar_Za_Gitare.Tests.Api;

public class LookupApiTests : ApiTestBase
{
    public LookupApiTests(CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    public static IEnumerable<object[]> LookupEndpoints()
    {
        yield return new object[] { "brands", "Nova marka" };
        yield return new object[] { "guitar-types", "Novi tip gitare" };
        yield return new object[] { "repair-statuses", "Novi status" };
        yield return new object[] { "repair-types", "Nova vrsta popravka" };
    }

    [Theory]
    [MemberData(nameof(LookupEndpoints))]
    public async Task GetAll_ReturnsOkAndItems(string route, string _)
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync($"/api/{route}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.Content.ReadFromJsonAsync<List<LookupDto>>();
        items.Should().NotBeNull();
        items.Should().NotBeEmpty();
    }

    [Theory]
    [MemberData(nameof(LookupEndpoints))]
    public async Task GetById_WhenItemExists_ReturnsOkAndItem(string route, string _)
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync($"/api/{route}/1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var item = await response.Content.ReadFromJsonAsync<LookupDto>();
        item.Should().NotBeNull();
        item!.Id.Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(LookupEndpoints))]
    public async Task GetById_WhenItemDoesNotExist_ReturnsNotFound(string route, string _)
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync($"/api/{route}/{TestDataSeeder.MissingIntId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [MemberData(nameof(LookupEndpoints))]
    public async Task Create_WhenRequestIsValid_ReturnsCreated(string route, string name)
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var client = CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync($"/api/{route}", new LookupRequestDto { Naziv = name });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var item = await response.Content.ReadFromJsonAsync<LookupDto>();
        item.Should().NotBeNull();
        item!.Naziv.Should().Be(name);
    }

    [Theory]
    [MemberData(nameof(LookupEndpoints))]
    public async Task Create_WhenRequestIsInvalid_ReturnsBadRequest(string route, string _)
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var client = CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync($"/api/{route}", new LookupRequestDto { Naziv = string.Empty });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [MemberData(nameof(LookupEndpoints))]
    public async Task Update_WhenItemExists_ReturnsNoContent(string route, string name)
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var client = CreateAuthenticatedClient();

        var response = await client.PutAsJsonAsync($"/api/{route}/1", new LookupRequestDto { Naziv = name + " updated" });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Theory]
    [MemberData(nameof(LookupEndpoints))]
    public async Task Update_WhenItemDoesNotExist_ReturnsNotFound(string route, string name)
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var client = CreateAuthenticatedClient();

        var response = await client.PutAsJsonAsync($"/api/{route}/{TestDataSeeder.MissingIntId}", new LookupRequestDto { Naziv = name });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [MemberData(nameof(LookupEndpoints))]
    public async Task Update_WhenRequestIsInvalid_ReturnsBadRequest(string route, string _)
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var client = CreateAuthenticatedClient();

        var response = await client.PutAsJsonAsync($"/api/{route}/1", new LookupRequestDto { Naziv = string.Empty });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [MemberData(nameof(LookupEndpoints))]
    public async Task Delete_WhenItemExistsAndUserIsAdmin_ReturnsNoContent(string route, string name)
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var client = CreateAuthenticatedClient(IdentitySeed.AdminRole);
        var createResponse = await client.PostAsJsonAsync($"/api/{route}", new LookupRequestDto { Naziv = name + " delete" });
        var created = await createResponse.Content.ReadFromJsonAsync<LookupDto>();

        var response = await client.DeleteAsync($"/api/{route}/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Theory]
    [MemberData(nameof(LookupEndpoints))]
    public async Task Delete_WhenItemDoesNotExistAndUserIsAdmin_ReturnsNotFound(string route, string _)
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var client = CreateAuthenticatedClient(IdentitySeed.AdminRole);

        var response = await client.DeleteAsync($"/api/{route}/{TestDataSeeder.MissingIntId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [MemberData(nameof(LookupEndpoints))]
    public async Task Delete_WhenUserIsManager_ReturnsForbidden(string route, string _)
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var client = CreateAuthenticatedClient(IdentitySeed.ManagerRole);

        var response = await client.DeleteAsync($"/api/{route}/1");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
