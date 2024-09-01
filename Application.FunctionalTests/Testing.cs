﻿using Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using WebApi.Services;

namespace Application.FunctionalTests
{
    public partial class Testing : IAsyncLifetime
    {
        private static ITestDatabase _database = null!;
        private static CustomWebApplicationFactory _factory = null!;
        private static IServiceScopeFactory _scopeFactory = null!;
        

        public Testing()
        {
            _database = TestDatabaseFactory.CreateAsync().Result;

            _factory = new CustomWebApplicationFactory(_database.GetConnection(), ClearUser());

            _scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();

        }
        protected static void SetCurrentUser(IUser user)
        {
            _factory = new CustomWebApplicationFactory(_database.GetConnection(), user);
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

            //_userId = null;
        }
        private IUser ClearUser()
        {
            var user = Mock.Of<IUser>(m => m.Username == "" && m.Id == "");
            return user;
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

        //public static string? GetUserId()
        //{
        //    return _userId;
        //}
        //public static void SetUserId(string userId)
        //{
        //    _userId= userId;
            
        //}

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
