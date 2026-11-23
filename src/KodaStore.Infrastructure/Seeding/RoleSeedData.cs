using KodaStore.Application.Security;
using KodaStore.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace KodaStore.Infrastructure.Seeding;

public static class RoleSeedData
{
    public static async Task SeedAsync(RoleManager<IdentityRole<Guid>> roleManager, UserManager<AppUser> userManager, string? bootstrapAdminEmail)
    {
        string[] requiredRoles = [
            AppRoles.Customer,
            AppRoles.Admin
        ];

        foreach (var roleName in requiredRoles)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(new IdentityRole<Guid>
            {
                Name = roleName
            });

            EnsureSucceeded(
                result,
                $"Membuat role {roleName}");
        }

        var existingUser = await userManager.Users.ToListAsync();

        foreach (var user in existingUser)
        {
            var roles = await userManager.GetRolesAsync(user);

            if (roles.Count > 0) continue;

            var result = await userManager.AddToRoleAsync(user, AppRoles.Customer);

            EnsureSucceeded(result, $"Adding CUSTOMER role to {user.Id}");
        }

        if (string.IsNullOrWhiteSpace(bootstrapAdminEmail)) return;

        var admin = await userManager.FindByEmailAsync(bootstrapAdminEmail.Trim()) ?? throw new InvalidOperationException("Bootstrap admin account is not registered");

        if (!await userManager.IsInRoleAsync(admin, AppRoles.Admin))
        {
            var result = await userManager.AddToRoleAsync(admin, AppRoles.Admin);

            EnsureSucceeded(result, "Adding role ADMIN");
        }

        if (await userManager.IsInRoleAsync(admin, AppRoles.Customer))
        {
            var result = await userManager.RemoveFromRoleAsync(admin, AppRoles.Customer);

            EnsureSucceeded(result, "Remove CUSTOMER role from admin");
        }
    }

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (result.Succeeded) return;

        var description = string.Join("; ", result.Errors.Select(err => err.Description));

        throw new InvalidOperationException($"{operation} Failed: {description}");
    }
}
