using Application.Common.Interfaces;

namespace Application.Roles;
public record GetRoleMatrixQuery:IRequest<RoleMatrix>
{
}
internal class GetRoleMatrixQueryHandler : IRequestHandler<GetRoleMatrixQuery, RoleMatrix>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityRoleServices _roleServices;
    private readonly IMapper _mapper;
    public GetRoleMatrixQueryHandler(IApplicationDbContext context, IIdentityRoleServices roleServices, IMapper mapper)
    {
        _context = context;
        _roleServices = roleServices;
        _mapper = mapper;
    }
    public async Task<RoleMatrix> Handle(GetRoleMatrixQuery request, CancellationToken cancellationToken)
    {
        return new RoleMatrix
        {
            Roles = await _context.AppRoles.ProjectTo<AppRoleVm>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken),
            Permissions = await _roleServices.GetRoles(cancellationToken),
            RoleMappings = await _context.AppRolePermissions.ProjectTo<RoleMapping>(_mapper.ConfigurationProvider).ToListAsync(cancellationToken),
        };
    }
}