namespace ModularSaaS.Domain.Shared;

/// <summary>
/// Abstract base record for all Domain-Driven Design value objects.
/// Records provide structural value equality, immutability, and with-expression cloning out of the box.
/// </summary>
public abstract record ValueObject : IValueObject;
