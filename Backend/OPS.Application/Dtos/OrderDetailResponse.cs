namespace OPS.Application.Dtos;

public record OrderItemResponse(int Id, string ProductName, int Quantity, decimal UnitPrice);

public record InventortyCheckResponse(int Id, DateTimeOffset CheckDate, bool IsAvailable);

public record OrderDetailResponse(
    int Id, 
    string OrderNumber, 
    string CustomerName, 
    string? CustomerEmail, 
    DateTimeOffset OrderDate, 
    string Status, 
    decimal TotalAmount,
    List<OrderItemResponse> Items,
    List<InventortyCheckResponse> InventortyChecks);



