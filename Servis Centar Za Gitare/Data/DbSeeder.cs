using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Servis_Centar_Za_Gitare.Data.Mock;

namespace Servis_Centar_Za_Gitare.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // Apply pending migrations
            await context.Database.MigrateAsync();

            // If there is existing customer data assume DB is seeded
            if (await context.Stranke.AnyAsync())
                return;

            // Add employees (technicians and managers)
            if (GuitarServiceMockData.Technicians?.Count > 0)
                context.Tehnicari.AddRange(GuitarServiceMockData.Technicians);

            if (GuitarServiceMockData.Managers?.Count > 0)
                context.Zaposlenici.AddRange(GuitarServiceMockData.Managers);

            // Normalize DateTime kinds to UTC for PostgreSQL and add customers and guitars
            if (GuitarServiceMockData.Guitars != null)
            {
                foreach (var g in GuitarServiceMockData.Guitars)
                {
                    g.DatumZaprimanja = DateTime.SpecifyKind(g.DatumZaprimanja, DateTimeKind.Utc);
                }
            }

            if (GuitarServiceMockData.Customers?.Count > 0)
                context.Stranke.AddRange(GuitarServiceMockData.Customers);

            if (GuitarServiceMockData.Guitars?.Count > 0)
                context.Gitare.AddRange(GuitarServiceMockData.Guitars);

            // Add repairs
            if (GuitarServiceMockData.Repairs != null)
            {
                foreach (var r in GuitarServiceMockData.Repairs)
                {
                    r.DatumOtvaranja = DateTime.SpecifyKind(r.DatumOtvaranja, DateTimeKind.Utc);
                    r.DatumZatvaranja = DateTime.SpecifyKind(r.DatumZatvaranja, DateTimeKind.Utc);
                }

                if (GuitarServiceMockData.Repairs.Count > 0)
                    context.Nalozi.AddRange(GuitarServiceMockData.Repairs);
            }

            await context.SaveChangesAsync();
        }
    }
}
