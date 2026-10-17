using BTL_QuanLySinhVien.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using BTL_QuanLySinhVien.Models;

namespace BTL_QuanLySinhVien.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Tạo các Vai trò (Roles) mặc định cho doanh nghiệp/trường học
            string[] roleNames = { "Admin", "Teacher", "Student" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Tạo tài khoản Quản trị viên (Admin) mẫu mặc định
            var adminEmail = "admin@studentmgmt.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Hệ Thống Quản Trị",
                    EmailConfirmed = true
                };

                // Mật khẩu mẫu: Admin@123456
                var result = await userManager.CreateAsync(adminUser, "Admin@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}