using Application.Common.Models;

namespace Application.Common.Interfaces;
public interface IHttpRequestService
{
    Task<T> SendAsync<T>(ApiRequest apiRequest);
}
