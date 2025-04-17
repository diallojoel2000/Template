using Application.Common.Models.Enums;
using Application.Common.Models;
using Newtonsoft.Json;
using System.Text;

namespace Application.FunctionalTests.Setup;
public class HttpServices
{
    private readonly HttpClient client;
    public HttpServices(HttpClient client)
    {
            this.client=client;
    }
    public async Task<HttpResponseMessage> SendAsync(ApiRequest apiRequest)
    {
        var message = new HttpRequestMessage();
        message.Headers.Add("Accept", "application/json");
        message.RequestUri = new Uri(apiRequest.Url);
        client.DefaultRequestHeaders.Clear();
        if (apiRequest.Data != null)
        {
            message.Content = new StringContent(JsonConvert.SerializeObject(apiRequest.Data), Encoding.UTF8, "application/json");
        }
        if (!string.IsNullOrEmpty(apiRequest.AccessToken))
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiRequest.AccessToken);
        }
        foreach (var header in apiRequest.Headers)
        {
            client.DefaultRequestHeaders.Add(header.Key, header.Value);
        }
        switch (apiRequest.ApiType)
        {
            case ApiType.POST:
                message.Method = HttpMethod.Post;
                break;
            case ApiType.GET:
                message.Method = HttpMethod.Get;
                break;
            case ApiType.PUT:
                message.Method = HttpMethod.Put;
                break;
            case ApiType.DELETE:
                message.Method = HttpMethod.Delete;
                break;
            default:
                message.Method = HttpMethod.Get;
                break;
        }
        
        var response = await client.SendAsync(message);
        return response;
    }
}
