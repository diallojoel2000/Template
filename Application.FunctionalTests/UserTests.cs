using Application.FunctionalTests.Setup;
using FluentAssertions;
using Application.Common.Models;
using Application.Users;
using Newtonsoft.Json;
using System.Net;

namespace Application.FunctionalTests;
public class UserTests:Testing
{
    [Theory]
    [MemberData(nameof(ValidPagesQueries))]
    public async Task CanGetUsers(GetPagedUsersQuery query)
    {
        await SeedDatabase();

        var token = await GetToken();

        var request = new ApiRequest
        {
            Url = $"{_client.BaseAddress}Users",
            Data = query,
            AccessToken = token,
            Method=HttpMethod.Get
        };

        var response = await new HttpServices(_client).SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonString = await response.Content.ReadAsStringAsync();
        var users = JsonConvert.DeserializeObject<PaginatedList<UsersVm>>(jsonString);
        users.Should().NotBeNull();
        users!.Items.Count.Should().BeGreaterThan(0);
    }
    [Fact]
    public async Task CanReturnEmpty()
    {
        await SeedDatabase();

        var token = await GetToken();

        var query = new GetPagedUsersQuery
        {
            PageNumber = 1,
            PageSize = 2,
            Search = "DoesNotExist"
        };

        var request = new ApiRequest
        {
            Url = $"{_client.BaseAddress}Users",
            Data = query,
            AccessToken = token,
            Method = HttpMethod.Get
        };

        var response = await new HttpServices(_client).SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonString = await response.Content.ReadAsStringAsync();
        var users = JsonConvert.DeserializeObject<PaginatedList<UsersVm>>(jsonString);
        users.Should().NotBeNull();
        users!.Items.Count.Should().Be(0);
    }

    public static IEnumerable<object[]> ValidPagesQueries => new List<object[]>
    {
        new object[]
        {
            new GetPagedUsersQuery
            {
                PageNumber = 1,
                PageSize = 2,
                Search ="admin@localhost"
             },
        },
        new object[]
        {
            new GetPagedUsersQuery
            {
                PageNumber = 1,
                PageSize = 2,
             },
        },
    };
}
