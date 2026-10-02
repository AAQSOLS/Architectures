namespace ModularSaaS.Domain.Shared;

public abstract class AuditableEntity : BaseEntity
{
    public DateTimeOffset CreatedAtUtc { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTimeOffset? ModifiedAtUtc { get; set; }

    public Guid? ModifiedBy { get; set; }
}
