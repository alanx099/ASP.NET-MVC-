using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Servis_Centar_Za_Gitare.Data;
using Servis_Centar_Za_Gitare.Dtos;
using Servis_Centar_Za_Gitare.models;
using Servis_Centar_Za_Gitare.Tests.Infrastructure;
using Xunit;

namespace Servis_Centar_Za_Gitare.Tests.Api;

public class CustomersApiTests : ApiTestBase
{
    public CustomersApiTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAll_ReturnsOkAndCustomers()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient().GetAsync("/api/customers");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var customers = await response.Content.ReadFromJsonAsync<List<CustomerDto>>();
        customers.Should().ContainSingle(customer => customer.Id == TestDataSeeder.ExistingCustomerId);
    }

    [Fact]
    public async Task GetById_WhenCustomerExists_ReturnsOkAndCustomer()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient().GetAsync($"/api/customers/{TestDataSeeder.ExistingCustomerId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var customer = await response.Content.ReadFromJsonAsync<CustomerDto>();
        customer!.Email.Should().Be("test.kupac@example.com");
    }

    [Fact]
    public async Task GetById_WhenCustomerDoesNotExist_ReturnsNotFound()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient().GetAsync($"/api/customers/{TestDataSeeder.MissingLongId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WhenRequestIsValid_ReturnsCreatedAndPersistsCustomer()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var request = ValidRequest("novi.kupac@example.com");

        var response = await CreateAuthenticatedClient().PostAsJsonAsync("/api/customers", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CustomerDto>();
        created!.Email.Should().Be(request.Email);
        var saved = await FindAsync<Stranka>(created.Id);
        saved!.Email.Should().Be(request.Email);
    }

    [Fact]
    public async Task Create_WhenRequestIsInvalid_ReturnsBadRequest()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var request = ValidRequest("bad@example.com");
        request.DatumRegistracije = "not-a-date";

        var response = await CreateAuthenticatedClient().PostAsJsonAsync("/api/customers", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WhenCustomerExists_ReturnsNoContentAndPersistsChanges()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var request = ValidRequest("updated.kupac@example.com");
        request.Ime = "Updated";

        var response = await CreateAuthenticatedClient().PutAsJsonAsync($"/api/customers/{TestDataSeeder.ExistingCustomerId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var saved = await FindAsync<Stranka>(TestDataSeeder.ExistingCustomerId);
        saved!.Ime.Should().Be("Updated");
        saved.Email.Should().Be(request.Email);
    }

    [Fact]
    public async Task Update_WhenCustomerDoesNotExist_ReturnsNotFound()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient().PutAsJsonAsync($"/api/customers/{TestDataSeeder.MissingLongId}", ValidRequest("missing@example.com"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WhenRequestIsInvalid_ReturnsBadRequest()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var request = ValidRequest("invalid@example.com");
        request.PoslovnicaId = TestDataSeeder.MissingLongId;

        var response = await CreateAuthenticatedClient().PutAsJsonAsync($"/api/customers/{TestDataSeeder.ExistingCustomerId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_WhenCustomerExistsAndUserIsAdmin_ReturnsNoContent()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var client = CreateAuthenticatedClient(IdentitySeed.AdminRole);
        var createResponse = await client.PostAsJsonAsync("/api/customers", ValidRequest("delete.kupac@example.com"));
        var created = await createResponse.Content.ReadFromJsonAsync<CustomerDto>();

        var response = await client.DeleteAsync($"/api/customers/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await FindAsync<Stranka>(created.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Delete_WhenCustomerDoesNotExistAndUserIsAdmin_ReturnsNotFound()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient(IdentitySeed.AdminRole).DeleteAsync($"/api/customers/{TestDataSeeder.MissingLongId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WhenUserIsManager_ReturnsForbidden()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient().DeleteAsync($"/api/customers/{TestDataSeeder.ExistingCustomerId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static CustomerRequestDto ValidRequest(string email) => new()
    {
        Ime = "Novi",
        Prezime = "Kupac",
        Email = email,
        BrojTelefona = "+385911234567",
        Adresa = "Nova adresa 1",
        DatumRegistracije = "2026-06-10T10:00",
        Napomena = "Test",
        PoslovnicaId = TestDataSeeder.ExistingOfficeId
    };
}
