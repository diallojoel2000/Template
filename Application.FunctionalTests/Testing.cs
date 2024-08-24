using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application.FunctionalTests
{
    public partial class Testing : IAsyncLifetime
    {
        private static ITestDatabase _database = null!;
        private static CustomWebApplicationFactory _factory = null!;
        private static IServiceScopeFactory _scopeFactory = null!;
        private static string? _userId;

        public Testing()
        {
            _database = TestDatabaseFactory.CreateAsync().Result;

            _factory = new CustomWebApplicationFactory(_database.GetConnection());

            _scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();

        }

        public static async Task ResetState()
        {
            try
            {
                await _database.ResetAsync();
            }
            catch (Exception)
            {
            }

            _userId = null;
        }

        public static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
        {
            using var scope = _scopeFactory.CreateScope();

            var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            return await mediator.Send(request);
        }

        public static async Task SendAsync(IBaseRequest request)
        {
            using var scope = _scopeFactory.CreateScope();

            var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

            await mediator.Send(request);
        }

        public static string? GetUserId()
        {
            return _userId;
        }
        
        public async Task InitializeAsync()
        {
            await ResetState();
        }

        async Task IAsyncLifetime.DisposeAsync()
        {
            await _database.DisposeAsync();
            await _factory.DisposeAsync();
        }
    }
}
