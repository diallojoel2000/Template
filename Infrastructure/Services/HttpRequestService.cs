using System.Text;
using System.Text.Encodings.Web;
using Application.Common.Interfaces;
using Application.Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Infrastructure.Services;
public class HttpRequestService : IHttpRequestService
{
    public IHttpClientFactory _httpClient { get; set; }
    private readonly ILogger<HttpRequestService> _logger;
    private readonly IConfiguration _configuration;
    HtmlEncoder _htmlEncoder;

    public HttpRequestService(IHttpClientFactory httpClient, ILogger<HttpRequestService> logger, IConfiguration configuration, HtmlEncoder htmlEncoder)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
        _htmlEncoder = htmlEncoder;
    }
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public async Task<T> SendAsync<T>(ApiRequest apiRequest)
    {
        var client = _httpClient.CreateClient();

        var message = new HttpRequestMessage();
        message.Headers.Add("Accept", "application/json");
        message.RequestUri = new Uri(apiRequest.Url);

        if (apiRequest.Data != null)
        {
            message.Content = new StringContent(JsonConvert.SerializeObject(apiRequest.Data), Encoding.UTF8, "application/json");
        }
        if (!string.IsNullOrEmpty(apiRequest.AccessToken))
        {
            message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiRequest.AccessToken);
        }
        foreach (var header in apiRequest.Headers)
        {
            message.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        message.Method = apiRequest.Method;

        _logger.LogInformation("Api Request: {@apiRequest}", apiRequest);
        var response = await client.SendAsync(message);
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();

            if (_configuration.GetValue<bool>("LogApiResponse"))
            {
                _logger.LogInformation("Api Response: {@apiResponse}", _htmlEncoder.Encode(json));
            }
            var apiResponseDto = JsonConvert.DeserializeObject<T>(json);
            return apiResponseDto;
        }
        else
        {
            _logger.LogError("Api Error: {@apiError}", response);
            throw new Exception(response?.ReasonPhrase);
        }
    }
}
