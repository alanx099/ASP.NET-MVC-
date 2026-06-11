using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Servis_Centar_Za_Gitare.Data;
using Servis_Centar_Za_Gitare.Dtos;
using Servis_Centar_Za_Gitare.Tests.Infrastructure;
using Xunit;

namespace Servis_Centar_Za_Gitare.Tests.Api;

public class MeApiTests : ApiTestBase
{
    public MeApiTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetGuitars_WhenUserHasCustomer_ReturnsOkAndGuitars()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient(IdentitySeed.UserRole).GetAsync("/api/me/guitars");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var guitars = await response.Content.ReadFromJsonAsync<List<MyGuitarDto>>();
        guitars.Should().ContainSingle(guitar => guitar.Id == TestDataSeeder.ExistingGuitarId);
    }

    [Fact]
    public async Task GetServiceOrders_WhenUserHasCustomer_ReturnsOkAndServiceOrders()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient(IdentitySeed.UserRole).GetAsync("/api/me/service-orders");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var repairs = await response.Content.ReadFromJsonAsync<List<MyServiceOrderDto>>();
        repairs.Should().ContainSingle(repair => repair.Id == TestDataSeeder.ExistingRepairId);
    }

    [Fact]
    public async Task GetGuitars_WhenUserIsNotAuthenticated_ReturnsUnauthorized()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await Factory.CreateClient().GetAsync("/api/me/guitars");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetGuitars_WhenUserRoleIsManager_ReturnsForbidden()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient(IdentitySeed.ManagerRole).GetAsync("/api/me/guitars");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
