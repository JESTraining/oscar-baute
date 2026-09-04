namespace OPS.Application.Events;

public record OrderCreatedEvent(int OrderId, string OrderNumber);