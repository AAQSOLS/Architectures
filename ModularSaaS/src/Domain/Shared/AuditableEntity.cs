namespace ModularSaaS.Domain.Shared;

public abstract class AuditableEntity : BaseEntity, ISoftDeletable
{
    public DateTimeOffset CreatedAtUtc { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTimeOffset? ModifiedAtUtc { get; set; }

    public Guid? ModifiedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTimeOffset? DeletedAtUtc { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual void SoftDelete(Guid? deletedBy, DateTimeOffset now)
    {
        IsDeleted = true;
        DeletedAtUtc = now;
        DeletedBy = deletedBy;
    }

    public virtual void Restore()
    {
        IsDeleted = false;
        DeletedAtUtc = null;
        DeletedBy = null;
    }
}
