namespace OPS.Application.Dtos;

public record OrderItemRequest(string ProductName, int Quantity, decimal UnitPrice);
