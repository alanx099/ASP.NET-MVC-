using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Servis_Centar_Za_Gitare.Data;
using Servis_Centar_Za_Gitare.Dtos;
using Servis_Centar_Za_Gitare.models;
using Servis_Centar_Za_Gitare.Tests.Infrastructure;
using Xunit;

namespace Servis_Centar_Za_Gitare.Tests.Api;

public class EmployeesApiTests : ApiTestBase
{
    public EmployeesApiTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAll_ReturnsOkAndEmployees()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient().GetAsync("/api/employees");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var employees = await response.Content.ReadFromJsonAsync<List<EmployeeDto>>();
        employees.Should().ContainSingle(employee => employee.Id == TestDataSeeder.ExistingEmployeeId);
    }

    [Fact]
    public async Task GetById_WhenEmployeeExists_ReturnsOkAndEmployee()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient().GetAsync($"/api/employees/{TestDataSeeder.ExistingEmployeeId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var employee = await response.Content.ReadFromJsonAsync<EmployeeDto>();
        employee!.Email.Should().Be("test.zaposlenik@example.com");
    }

    [Fact]
    public async Task GetById_WhenEmployeeDoesNotExist_ReturnsNotFound()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient().GetAsync($"/api/employees/{TestDataSeeder.MissingLongId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WhenRequestIsValid_ReturnsCreatedAndPersistsEmployee()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var request = ValidRequest("new.employee@example.com");

        var response = await CreateAuthenticatedClient().PostAsJsonAsync("/api/employees", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<EmployeeDto>();
        created!.Email.Should().Be(request.Email);
        (await FindAsync<Zaposlenik>(created.Id))!.Email.Should().Be(request.Email);
    }

    [Fact]
    public async Task Create_WhenRequestIsInvalid_ReturnsBadRequest()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var request = ValidRequest("invalid.employee@example.com");
        request.DatumZaposlenja = "not-a-date";

        var response = await CreateAuthenticatedClient().PostAsJsonAsync("/api/employees", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WhenEmployeeExists_ReturnsNoContentAndPersistsChanges()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var request = ValidRequest("updated.employee@example.com");
        request.Ime = "Updated";

        var response = await CreateAuthenticatedClient().PutAsJsonAsync($"/api/employees/{TestDataSeeder.ExistingEmployeeId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var saved = await FindAsync<Zaposlenik>(TestDataSeeder.ExistingEmployeeId);
        saved!.Ime.Should().Be("Updated");
    }

    [Fact]
    public async Task Update_WhenEmployeeDoesNotExist_ReturnsNotFound()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient().PutAsJsonAsync($"/api/employees/{TestDataSeeder.MissingLongId}", ValidRequest("missing.employee@example.com"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WhenRequestIsInvalid_ReturnsBadRequest()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var request = ValidRequest("bad.employee@example.com");
        request.PoslovnicaId = TestDataSeeder.MissingLongId;

        var response = await CreateAuthenticatedClient().PutAsJsonAsync($"/api/employees/{TestDataSeeder.ExistingEmployeeId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_WhenEmployeeExistsAndUserIsAdmin_ReturnsNoContent()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient(IdentitySeed.AdminRole).DeleteAsync($"/api/employees/{TestDataSeeder.ExistingEmployeeId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await FindAsync<Zaposlenik>(TestDataSeeder.ExistingEmployeeId)).Should().BeNull();
    }

    [Fact]
    public async Task Delete_WhenEmployeeDoesNotExistAndUserIsAdmin_ReturnsNotFound()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient(IdentitySeed.AdminRole).DeleteAsync($"/api/employees/{TestDataSeeder.MissingLongId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static EmployeeRequestDto ValidRequest(string email) => new()
    {
        Ime = "Novi",
        Prezime = "Zaposlenik",
        Email = email,
        BrojTelefona = "+385921234567",
        Adresa = "Nova adresa zaposlenika",
        DatumZaposlenja = "2026-06-10T09:00",
        Placa = 1400,
        PoslovnicaId = TestDataSeeder.ExistingOfficeId
    };
}
