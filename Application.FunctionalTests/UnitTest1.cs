using Application.Common.Exceptions;
using Application.FunctionalTests.Setup;
using Application.SampleEntities.Create;
using FluentAssertions;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace Application.FunctionalTests
{
    public class UnitTest1:Testing
    {
        [Fact]
        public async Task ShouldReturnValidationException()
        {
            var command = new CreateSampleRequest
            {
                Name = "",
                Description = "Test Description"
            };
            await FluentActions.Invoking(() =>
           SendAsync(command)).Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task ShouldReturnOkWhileAuthenticated()
        {
            await SeedDatabase();
            var command = new CreateSampleRequest
            {
                Name = "Sample Name",
                Description = "Test Description"
            };
            var token = await GetToken();
            var message = new HttpRequestMessage();
            message.Headers.Add("Accept", "application/json");
            message.RequestUri = new Uri($"{_client.BaseAddress}SampleDatas");
            message.Content = new StringContent(JsonConvert.SerializeObject(command), Encoding.UTF8, "application/json");
            message.Method = HttpMethod.Post;

            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _client.SendAsync(message);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

        }

    }
}