using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Servis_Centar_Za_Gitare.Data;
using Servis_Centar_Za_Gitare.Tests.Infrastructure;
using Xunit;

namespace Servis_Centar_Za_Gitare.Tests.Api;

public abstract class ApiTestBase : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly CustomWebApplicationFactory Factory;

    protected ApiTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
    }

    protected HttpClient CreateAuthenticatedClient(string role = IdentitySeed.ManagerRole)
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeaderName, role);
        return client;
    }

    protected async Task<T?> FindAsync<T>(params object[] keyValues)
        where T : class
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var entity = await db.Set<T>().FindAsync(keyValues);
        if (entity != null)
        {
            db.Entry(entity).State = EntityState.Detached;
        }

        return entity;
    }

    protected async Task<int> CountAsync<T>()
        where T : class
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.Set<T>().CountAsync();
    }
}
