using GymManagement.DAL.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagement.DAL.Data.DataSeeding
{
    public static class IdentityDataSeeding
    {
        public static async Task SeedIdentityDataAsync(RoleManager<IdentityRole> roleManager, 
                                                 UserManager<ApplicationUser> userManager,
                                                 ILogger logger,
                                                 CancellationToken ct = default)
        {
            try
            {
                var hasUsers = await userManager.Users.AnyAsync(ct);
                var hasRoles = await roleManager.Roles.AnyAsync(ct);

                if (hasRoles && hasUsers) return;

                var roles = new List<IdentityRole>()
            {
                new IdentityRole() {Name = "SuperAdmin"},
                new IdentityRole() {Name = "Admin"}
            };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role.Name!))
                    {
                        var roleResult = await roleManager.CreateAsync(role);
                        if (!roleResult.Succeeded)
                        {
                            logger.LogError($"Failed To Create Role {role.Name} : {string.Join(" ; ", roleResult.Errors.Select(r => r.Description))}");
                        }
                    }
                }

                if (!hasUsers)
                {
                    var mainAdmin = new ApplicationUser()
                    {
                        FirstName = "Mohamed",
                        LastName = "Ramadan",
                        Email = "mohamedramadan@gmail.com",
                        UserName = "mohamed12ramdan",
                        PhoneNumber = "01011123423"
                    };

                    await userManager.CreateAsync(mainAdmin, "P@ssw0rd");
                    await userManager.AddToRoleAsync(mainAdmin, "SuperAdmin");

                    var admin = new ApplicationUser()
                    {
                        FirstName = "Ramy",
                        LastName = "Ramadan",
                        Email = "ramyramadan@gmail.com",
                        UserName = "ramy12ramdan",
                        PhoneNumber = "01011433423"
                    };
                    await userManager.CreateAsync(admin, "P@ssw0rd");
                    await userManager.AddToRoleAsync(admin, "Admin");

                    logger.LogInformation("Identity Data Sedded");

                    return;
                }
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Identity Seeding Failed");
                return;
            }
        }
    }
}
