namespace ModularSaaS.Domain.Shared;

public interface ISoftDeletable
{
    public bool IsDeleted { get; set; }

    public DateTimeOffset? DeletedAtUtc { get; set; }

    public Guid? DeletedBy { get; set; }
}
