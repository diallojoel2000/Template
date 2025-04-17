using Application.Common.Interfaces;
using Application.Common.Models;
using Newtonsoft.Json;


namespace Application.Users;
public record GetPagedUsersQuery:IRequest<PaginatedList<UsersVm>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Search { get; set; }
}
internal class GetPagedUsersQueryHandller : IRequestHandler<GetPagedUsersQuery, PaginatedList<UsersVm>>
{
    private readonly IIdentityService _identityService;
    
    public GetPagedUsersQueryHandller(IIdentityService identityService)
    {
        _identityService = identityService;
    }
    public async Task<PaginatedList<UsersVm>> Handle(GetPagedUsersQuery request, CancellationToken cancellationToken)
    {
        var response = await _identityService.GetUsers(request.PageNumber, request.PageSize, request.Search);
        var pagedList = JsonConvert.DeserializeObject<PaginatedList<UsersVm>>(response.Result.ToString());
        return pagedList;
    }
}