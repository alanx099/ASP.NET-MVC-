using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Servis_Centar_Za_Gitare.Data;
using Servis_Centar_Za_Gitare.Dtos;
using Servis_Centar_Za_Gitare.models;
using Servis_Centar_Za_Gitare.Tests.Infrastructure;
using Xunit;

namespace Servis_Centar_Za_Gitare.Tests.Api;

public class KnowledgeApiTests : ApiTestBase
{
    public KnowledgeApiTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAll_ReturnsOkAndKnowledge()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient().GetAsync("/api/knowledge");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var knowledge = await response.Content.ReadFromJsonAsync<List<KnowledgeDto>>();
        knowledge.Should().ContainSingle(item => item.TehnicarId == TestDataSeeder.ExistingTechnicianId);
    }

    [Fact]
    public async Task GetById_WhenKnowledgeExists_ReturnsOkAndKnowledge()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient().GetAsync($"/api/knowledge/{TestDataSeeder.ExistingTechnicianId}/{TestDataSeeder.ExistingGuitarTypeId}/{TestDataSeeder.ExistingRepairTypeId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var knowledge = await response.Content.ReadFromJsonAsync<KnowledgeDto>();
        knowledge!.TehnicarId.Should().Be(TestDataSeeder.ExistingTechnicianId);
    }

    [Fact]
    public async Task GetById_WhenKnowledgeDoesNotExist_ReturnsNotFound()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient().GetAsync($"/api/knowledge/{TestDataSeeder.ExistingTechnicianId}/{TestDataSeeder.SecondGuitarTypeId}/{TestDataSeeder.SecondRepairTypeId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WhenRequestIsValid_ReturnsCreatedAndPersistsKnowledge()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var request = new KnowledgeRequestDto { TehnicarId = TestDataSeeder.ExistingTechnicianId, TipGitareId = TestDataSeeder.SecondGuitarTypeId, VrstaPopravkaId = TestDataSeeder.SecondRepairTypeId };

        var response = await CreateAuthenticatedClient().PostAsJsonAsync("/api/knowledge", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        (await FindAsync<Znanje>(request.TehnicarId, request.TipGitareId, request.VrstaPopravkaId)).Should().NotBeNull();
    }

    [Fact]
    public async Task Create_WhenRequestIsInvalid_ReturnsBadRequest()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var request = new KnowledgeRequestDto { TehnicarId = TestDataSeeder.MissingLongId, TipGitareId = TestDataSeeder.ExistingGuitarTypeId, VrstaPopravkaId = TestDataSeeder.ExistingRepairTypeId };

        var response = await CreateAuthenticatedClient().PostAsJsonAsync("/api/knowledge", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WhenKnowledgeExists_ReturnsNoContentAndPersistsChanges()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var request = new KnowledgeRequestDto { TehnicarId = TestDataSeeder.ExistingTechnicianId, TipGitareId = TestDataSeeder.SecondGuitarTypeId, VrstaPopravkaId = TestDataSeeder.SecondRepairTypeId };

        var response = await CreateAuthenticatedClient().PutAsJsonAsync($"/api/knowledge/{TestDataSeeder.ExistingTechnicianId}/{TestDataSeeder.ExistingGuitarTypeId}/{TestDataSeeder.ExistingRepairTypeId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await FindAsync<Znanje>(request.TehnicarId, request.TipGitareId, request.VrstaPopravkaId)).Should().NotBeNull();
    }

    [Fact]
    public async Task Update_WhenKnowledgeDoesNotExist_ReturnsNotFound()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var request = new KnowledgeRequestDto { TehnicarId = TestDataSeeder.ExistingTechnicianId, TipGitareId = TestDataSeeder.SecondGuitarTypeId, VrstaPopravkaId = TestDataSeeder.SecondRepairTypeId };

        var response = await CreateAuthenticatedClient().PutAsJsonAsync($"/api/knowledge/{TestDataSeeder.ExistingTechnicianId}/{TestDataSeeder.SecondGuitarTypeId}/{TestDataSeeder.SecondRepairTypeId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_WhenRequestIsInvalid_ReturnsBadRequest()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var request = new KnowledgeRequestDto { TehnicarId = TestDataSeeder.ExistingTechnicianId, TipGitareId = TestDataSeeder.MissingIntId, VrstaPopravkaId = TestDataSeeder.SecondRepairTypeId };

        var response = await CreateAuthenticatedClient().PutAsJsonAsync($"/api/knowledge/{TestDataSeeder.ExistingTechnicianId}/{TestDataSeeder.ExistingGuitarTypeId}/{TestDataSeeder.ExistingRepairTypeId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_WhenKnowledgeExistsAndUserIsAdmin_ReturnsNoContent()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient(IdentitySeed.AdminRole).DeleteAsync($"/api/knowledge/{TestDataSeeder.ExistingTechnicianId}/{TestDataSeeder.ExistingGuitarTypeId}/{TestDataSeeder.ExistingRepairTypeId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await FindAsync<Znanje>(TestDataSeeder.ExistingTechnicianId, TestDataSeeder.ExistingGuitarTypeId, TestDataSeeder.ExistingRepairTypeId)).Should().BeNull();
    }

    [Fact]
    public async Task Delete_WhenKnowledgeDoesNotExistAndUserIsAdmin_ReturnsNotFound()
    {
        await TestDataSeeder.SeedApiDataAsync(Factory);
        var response = await CreateAuthenticatedClient(IdentitySeed.AdminRole).DeleteAsync($"/api/knowledge/{TestDataSeeder.ExistingTechnicianId}/{TestDataSeeder.SecondGuitarTypeId}/{TestDataSeeder.SecondRepairTypeId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
