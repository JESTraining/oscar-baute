using OPS.Application.Events;

namespace OPS.Application.Interfaces;

public interface IEventPublisher
{
    void Publish(OrderCreatedEvent evt);
}