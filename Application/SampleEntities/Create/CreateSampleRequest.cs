using Application.Common.Interfaces;
using Domain.Entities;


namespace Application.SampleEntities.Create;

public record CreateSampleRequest : IRequest<long>
{
    public string Name { get; init; } = null!;
    public string Description { get; init; } = null!;
}
internal class CreateSampleRequestHandler : IRequestHandler<CreateSampleRequest,long>
{
    private readonly IApplicationDbContext _context;
    public CreateSampleRequestHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<long> Handle(CreateSampleRequest request, CancellationToken cancellationToken)
    {
        var sample = new SampleEntity
        {
            Name = request.Name,
            Description = request.Description,
        };
        _context.SampleEntities.Add(sample);
        await _context.SaveChangesAsync(cancellationToken);
        return sample.Id;
    }

}
