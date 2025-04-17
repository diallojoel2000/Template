using Application.Authentication.Commands;
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Common.Models.Enums;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Text;
using static Infrastructure.Persistence.InitialiserExtensions;

namespace Application.FunctionalTests.Setup
{
    [CollectionDefinition("Sequential Tests", DisableParallelization = true)]
    [Collection("Sequential Tests")]
    public partial class Testing : IAsyncLifetime
    {
        private static ITestDatabase _database = null!;
        private static CustomWebApplicationFactory _factory = null!;
        private static IServiceScopeFactory _scopeFactory = null!;
        public static HttpClient _client = null!;
        public static IEncryptionService _encryptionService = null!;

        public async Task InitializeAsync()
        {
            _database = await TestDatabaseFactory.CreateAsync();

            _factory = new CustomWebApplicationFactory(_database.GetConnection());

            _scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
            _client = _factory.CreateClient();
            _encryptionService = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<IEncryptionService>();

            await ResetState();
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
        
        public static async Task<string> GetToken()
        {
            var request = new ApiRequest
            {
                ApiType = ApiType.POST,
                Data = new LoginCommand
                {
                    Username = _encryptionService.EncryptAes("admin@localhost"),
                    Password = _encryptionService.EncryptAes("Administrator1!")
                },
                Url = $"{_client.BaseAddress}Authentication/Login"
            };
            var response = await new HttpServices(_client).SendAsync(request);
            var jsonString = await response.Content.ReadAsStringAsync();
            var responseDto = JsonConvert.DeserializeObject<ResponseDto>(jsonString);
            var tokenVm = JsonConvert.DeserializeObject<TokenVm>(responseDto!.Result.ToString());
            return tokenVm!.Token;
        }
        public static async Task SeedDatabase()
        {
            using var scope = _scopeFactory.CreateScope();
            var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();
            await initialiser.SeedAsync();
        }

        async Task IAsyncLifetime.DisposeAsync()
        {
            await _database.DisposeAsync();
            await _factory.DisposeAsync();
        }
    }
}
