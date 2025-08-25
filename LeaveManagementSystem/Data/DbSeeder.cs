using LeaveManagementSystem.Models;
using Microsoft.AspNetCore.Identity;

namespace LeaveManagementSystem.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<Employee>>();

            // Seed Roles
            string[] roles = { "Admin", "HR", "Manager", "Employee" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed Admin User
            if (await userManager.FindByEmailAsync("admin@company.com") == null)
            {
                var admin = new Employee
                {
                    UserName = "admin@company.com",
                    Email = "admin@company.com",
                    FirstName = "System",
                    LastName = "Administrator",
                    EmployeeId = "ADM001",
                    Department = "IT",
                    Designation = "System Admin",
                    Address ="Kanchanpur Matiyari",
                    DateOfJoining = DateTime.Now,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }
    }
}

