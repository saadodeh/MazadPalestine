namespace MzadPalestine.Core.Events;

public abstract class BaseDomainEvent
{
    public DateTime OccurredOn { get; protected set; }

    protected BaseDomainEvent()
    {
        OccurredOn = DateTime.UtcNow;
    }
}
