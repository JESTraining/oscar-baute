using OPS.Application.Dtos;
using OPS.Domain.Entities;

namespace OPS.Application.Mappings;

public static class OrderMappings
{
    public static OrderListItemResponse ToListItemResponse(this Order order) => new(
        order.Id,
        order.OrderNumber,
        order.CustomerName,
        order.OrderDate.DateTime,
        order.Status.ToString(),
        order.TotalAmount);

    public static OrderDetailResponse ToDetailResponse(this Order order) => new(
        order.Id,
        order.OrderNumber,
        order.CustomerName,
        order.CustomerEmail,
        order.OrderDate.DateTime,
        order.Status.ToString(),
        order.TotalAmount,
        order.OrderItems
            .Select(i => new OrderItemResponse(i.Id, i.ProductName, i.Quantity, i.UnitPrice))
            .ToList(),
        order.InventoryChecks
            .Select(c => new InventoryCheckResponse(c.Id, c.CheckDate.DateTime, c.IsAvailable))
            .ToList());
}