namespace OPS.Application.Dtos;

public record OrderListItemResponse(int Id, string OrderNumber, string CustomerName, DateTimeOffset OrderDate, string Status, decimal TotalAmount);