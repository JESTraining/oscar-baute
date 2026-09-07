namespace  OPS.Application.Dtos;

public record CreateOrderRequest(string CustomerName, string? CustomerEmail, List<OrderItemRequest> Items);

