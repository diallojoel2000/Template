using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Common;

public abstract class BaseAuditableEntity : BaseEntity
{
    public DateTime Created { get; set; }
    [MaxLength(100)]
    public string? CreatedBy { get; set; }

    public DateTime? LastModified { get; set; }
    [MaxLength(100)]
    public string? LastModifiedBy { get; set; }
    [MaxLength(4000)]
    public string? JsonRequest { get; set; }
    public Status? JsonRequestStatus { get; set; }
}
