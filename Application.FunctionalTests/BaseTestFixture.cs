using Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Application.FunctionalTests
{
    //public class BaseTestFixture : IDisposable
    //{
    //    public ApplicationDbContext Context { get; private set; }
    //    private static CustomWebApplicationFactory _factory = null!;
    //    private static IServiceScopeFactory _scopeFactory = null!;
    //    private static string? _userId;

    //    public BaseTestFixture()
    //    {
    //        var configuration = new ConfigurationBuilder()
    //            .SetBasePath(AppContext.BaseDirectory)
    //            .AddJsonFile("appsettings.Test.json")
    //            .Build();

    //        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    //            .UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
    //               builder => builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
    //            .Options;

    //        Context = new ApplicationDbContext(options);
    //        Context.Database.EnsureCreated();

    //        _scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
    //    }

    //    public static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    //    {
    //        using var scope = _scopeFactory.CreateScope();

    //        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

    //        return await mediator.Send(request);
    //    }

    //    public static async Task SendAsync(IBaseRequest request)
    //    {
    //        using var scope = _scopeFactory.CreateScope();

    //        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

    //        await mediator.Send(request);
    //    }

    //    public static string? GetUserId()
    //    {
    //        return _userId;
    //    }
    //    public void Dispose()
    //    {
    //        Context.Database.EnsureDeleted();
    //        Context.Dispose();
    //    }

    //    // Other methods and properties (see the full example below)
    //}
}
