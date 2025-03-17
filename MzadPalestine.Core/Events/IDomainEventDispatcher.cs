namespace MzadPalestine.Core.Events;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(BaseDomainEvent domainEvent);
    Task DispatchAsync(IEnumerable<BaseDomainEvent> domainEvents);
}
