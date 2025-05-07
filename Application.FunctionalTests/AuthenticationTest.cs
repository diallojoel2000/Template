using Application.Authentication.Commands;
using Application.Common.Models;
using Application.FunctionalTests.Setup;
using FluentAssertions;
using Newtonsoft.Json;
using System.Net;
using System.Text;


namespace Application.FunctionalTests;
public class AuthenticationTest:Testing
{
    [Fact]
    public async Task ShouldAuthenticateSucessfully()
    {
        await SeedDatabase();
        var request = new ApiRequest
        {
            Data = new LoginCommand
            {
                Username = _encryptionService.EncryptAes("admin@localhost"),
                Password = _encryptionService.EncryptAes("Administrator1!")
            },
            Url = $"{_client.BaseAddress}Authentication/Login",
            Method = HttpMethod.Post
        };
        
        var response = await new HttpServices(_client).SendAsync(request);

        var json = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonConvert.DeserializeObject<ResponseDto>(json);
        apiResponse.Should().NotBeNull();
        apiResponse!.IsSuccess.Should().BeTrue();
        TokenVm tokenVm = JsonConvert.DeserializeObject<TokenVm>(apiResponse.Result.ToString());
        tokenVm.Should().NotBeNull();
        tokenVm.Token.Should().NotBeNull();
    }

    [Fact]
    public async Task ShouldReturnBadRequest()
    {
        var data = new LoginCommand
        {
            Username = "administrator@loca",
            Password = "123A"
        };

        var message = new HttpRequestMessage();
        message.Headers.Add("Accept", "application/json");
        message.RequestUri = new Uri($"{_client.BaseAddress}Authentication/Login");
        message.Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
        message.Method = HttpMethod.Post;

        var response = await _client.SendAsync(message);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

    }
}
