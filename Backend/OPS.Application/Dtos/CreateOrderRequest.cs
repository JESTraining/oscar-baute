namespace  OPS.Application.Dtos;

public record CreateOrderRequest(string CustomerNAme, string? CustomerEmail, List<OrderItemRequest> Items);

