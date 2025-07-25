﻿using Domain.Common;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace Infrastructure.Persistence;
public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();

        await initialiser.SeedAsync();
    }
    public class ApplicationDbContextInitialiser
    {
        private readonly ILogger<ApplicationDbContextInitialiser> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task InitialiseAsync()
        {
            try
            {
                await _context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initialising the database.");
                throw;
            }
        }

        public async Task SeedAsync()
        {
            try
            {
                await TrySeedAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }

        public async Task TrySeedAsync()
        {
            //await CreateApplicationClasses();
            // Default roles
            var manageUserRole = new IdentityRole("ManageUsers");
            var manageRole = new IdentityRole("ManageRoles");

            if (_roleManager.Roles.All(r => r.Name != manageUserRole.Name))
            {
                await _roleManager.CreateAsync(manageUserRole);
            }
            if (_roleManager.Roles.All(r => r.Name != manageRole.Name))
            {
                await _roleManager.CreateAsync(manageRole);
            }

            // Default users
            var administrator = new ApplicationUser {FullName="John Doe", UserName = "admin@localhost", Email = "admin@localhost" };

            if (_userManager.Users.All(u => u.UserName != administrator.UserName))
            {
                await _userManager.CreateAsync(administrator, "Administrator1!");
                if (!string.IsNullOrWhiteSpace(manageUserRole.Name))
                {
                    await _userManager.AddToRolesAsync(administrator, new[] { manageUserRole.Name, manageRole.Name });
                }
            }

            // Default data
            // Seed, if necessary

        }

        public async Task CreateApplicationClasses()
        {
            var baseFolder = "\\GeneratedClasses";
            if (!Directory.Exists(baseFolder))
            {
                Directory.CreateDirectory(baseFolder);
            }
            var appFolder = $"{baseFolder}/Application{DateTime.Now.ToString("yyyyMMddmmss")}";
            Directory.CreateDirectory(appFolder);

            var entityTypes = _context.Model.GetEntityTypes();
            foreach (var entityType in entityTypes)
            {
                var entity = entityType.ClrType;
                if(entity.Name.StartsWith("Identity"))
                {
                    continue;
                }
                var properties = entityType.GetProperties();
                
                var entityFolder = $"{appFolder}/{entity.Name.Plural()}";
                Directory.CreateDirectory(entityFolder);
                var folderPath = Path.GetFullPath(entityFolder);

                CreateCRUD.CommandCreate(entity, properties, entityFolder);
                foreach (var item in entity.GetProperties())
                {
                    var prop = item.Name;
                }
            }
        }
    }
}