using Domain.Constants;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace NAQLAH.Server.SeedData
{
    /// <summary>
    /// Seeds NA_RoleClaims with default permissions for Admin role. Call from Program after Migrate.
    /// </summary>
    public static class RolePermissionsSeed
    {
        public static async Task SeedAdminRolePermissionsAsync(RoleManager<Role> roleManager)
        {
            var adminRole = await roleManager.FindByNameAsync(Role.Admin.Name);
            if (adminRole == null) return;

            var existingClaims = (await roleManager.GetClaimsAsync(adminRole))
                .Where(c => c.Type == PermissionNames.ClaimType)
                .Select(c => c.Value)
                .ToHashSet();

            foreach (var perm in PermissionNames.All)
            {
                if (existingClaims.Contains(perm.Name)) continue;
                await roleManager.AddClaimAsync(adminRole, new Claim(PermissionNames.ClaimType, perm.Name));
            }
        }
    }
}
