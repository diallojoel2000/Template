using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>,  IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    
    public DbSet<SampleEntity> SampleEntities => Set<SampleEntity>();
    public DbSet<AppRole> AppRoles => Set<AppRole>();
    public DbSet<AppRolePermission> AppRolePermissions => Set<AppRolePermission>();
    public DbSet<LoginLog> LoginLogs => Set<LoginLog>();

    

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}