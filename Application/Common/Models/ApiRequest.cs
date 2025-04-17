using Application.Common.Models.Enums;

namespace Application.Common.Models;
public class ApiRequest
{
    public string Url { get; set; } = null!;
    public dynamic? Data { get; set; }

    public ApiType ApiType { get; set; } = ApiType.GET;
    public string? AccessToken { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
}
