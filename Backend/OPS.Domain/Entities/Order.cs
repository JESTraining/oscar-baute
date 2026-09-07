using OPS.Domain.Common;

namespace OPS.Domain.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = default!;
    public string CustomerName { get; set; } = default!;
    public string? CustomerEmail { get; set; } = default!;
    public DateTimeOffset OrderDate { get; set; } = DateTimeOffset.Now;
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItem> OrderItems { get; set; } = new();
    public List<InventoryCheck> InventoryChecks = new();
}