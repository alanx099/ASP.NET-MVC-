using Microsoft.AspNetCore.Identity;
using Servis_Centar_Za_Gitare.models;
using Servis_Centar_Za_Gitare.Services;

namespace Servis_Centar_Za_Gitare.Data
{
    public static class IdentitySeed
    {
        public const string AdminRole = "Admin";
        public const string ManagerRole = "Manager";
        public const string UserRole = "User";

        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            var linker = services.GetRequiredService<CustomerAccountLinker>();

            await EnsureDefaultRolesAsync(roleManager);

            await EnsureUserAsync(userManager, linker, "admin@test.com", "Admin123!", AdminRole, "Admin", "User");
            await EnsureUserAsync(userManager, linker, "manager@test.com", "Manager123!", ManagerRole, "Manager", "User");
            await EnsureUserAsync(userManager, linker, "user@test.com", "User123!", UserRole, "Test", "User");
        }

        private static async Task EnsureUserAsync(
            UserManager<AppUser> userManager,
            CustomerAccountLinker linker,
            string email,
            string password,
            string role,
            string firstName,
            string lastName)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
                }
            }
            if (!await userManager.IsInRoleAsync(user, role))
            {
                var addRoleResult = await userManager.AddToRoleAsync(user, role);
                if (!addRoleResult.Succeeded)
                {
                    throw new InvalidOperationException(string.Join("; ", addRoleResult.Errors.Select(error => error.Description)));
                }
            }

            if (role == UserRole)
            {
                try
                {
                    await linker.LinkOrCreateCustomerAsync(user, firstName, lastName, "+385000000");
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"Customer link seed warning for {email}: {ex.Message}");
                }
            }
        }

        public static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string role)
        {
            if (await roleManager.RoleExistsAsync(role))
            {
                return;
            }

            var result = await roleManager.CreateAsync(new IdentityRole(role));
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
            }
        }

        public static async Task EnsureDefaultRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (var role in new[] { AdminRole, ManagerRole, UserRole })
            {
                await EnsureRoleAsync(roleManager, role);
            }
        }
    }
}
