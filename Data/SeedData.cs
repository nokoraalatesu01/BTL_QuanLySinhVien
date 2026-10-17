using BTL_QuanLySinhVien.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace BTL_QuanLySinhVien.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roleNames = { "Admin", "Teacher", "Student" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                    if (!roleResult.Succeeded)
                    {
                        throw new InvalidOperationException(
                            $"Failed to create role '{roleName}'. Identity error codes: {FormatErrorCodes(roleResult)}");
                    }
                }
            }

            var bootstrapEnabledValue = configuration["BootstrapAdmin:Enabled"];
            if (string.IsNullOrWhiteSpace(bootstrapEnabledValue))
            {
                return;
            }

            if (!bool.TryParse(bootstrapEnabledValue, out var bootstrapEnabled))
            {
                throw new InvalidOperationException("BootstrapAdmin:Enabled must be either true or false.");
            }

            if (!bootstrapEnabled)
            {
                return;
            }

            var adminEmail = configuration["BootstrapAdmin:Email"];
            if (string.IsNullOrWhiteSpace(adminEmail))
            {
                throw new InvalidOperationException(
                    "BootstrapAdmin:Email is required when bootstrap is enabled.");
            }

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser != null)
            {
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    throw new InvalidOperationException(
                        "Bootstrap Admin email matches an existing account that is not assigned the Admin role; no account changes were made.");
                }

                return;
            }

            var adminPassword = configuration["BootstrapAdmin:Password"];
            if (string.IsNullOrWhiteSpace(adminPassword))
            {
                throw new InvalidOperationException(
                    "BootstrapAdmin:Password is required to create the bootstrap Admin account.");
            }

            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Administrator"
            };

            var userResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (!userResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create the bootstrap Admin account. Identity error codes: {FormatErrorCodes(userResult)}");
            }

            var roleAssignmentResult = await userManager.AddToRoleAsync(adminUser, "Admin");
            if (!roleAssignmentResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to assign the Admin role to the bootstrap account. Identity error codes: {FormatErrorCodes(roleAssignmentResult)}");
            }
        }

        private static string FormatErrorCodes(IdentityResult result) =>
            string.Join(", ", result.Errors.Select(error => error.Code));
    }
}
