using Application.Authentication.Commands;
using Application.Common.Models;
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

        public async Task InitializeAsync()
        {
            _database = await TestDatabaseFactory.CreateAsync();

            _factory = new CustomWebApplicationFactory(_database.GetConnection());

            _scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
            _client = _factory.CreateClient();

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
            var data = new LoginCommand
            {
                Username = "administrator@localhost",
                Password = "Administrator1!"
            };

            var message = new HttpRequestMessage();
            message.Headers.Add("Accept", "application/json");
            message.RequestUri = new Uri($"{_client.BaseAddress}Authentication/Login");
            message.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            message.Method = HttpMethod.Post;

            var response = await _client.SendAsync(message);
            
            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ResponseDto>(json);
           
            TokenVm tokenVm = JsonConvert.DeserializeObject<TokenVm>(apiResponse.Result.ToString());
            return tokenVm.Token;
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
