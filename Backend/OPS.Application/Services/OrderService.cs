using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OPS.Application.Common.Helpers;
using OPS.Application.Dtos;
using OPS.Application.Events;
using OPS.Application.Interfaces;
using OPS.Application.Mappings;
using OPS.Domain.Entities;

namespace OPS.Application.Services;

/// <summary>
/// Implements the <see cref="IOrderService"/> interface for managing orders.
/// </summary>
public class OrderService : IOrderService
{
    private readonly IRepository<Order> orderRepository;
    private readonly IEventPublisher eventPublisher;
    private readonly ILogger<OrderService> logger;

    public OrderService(IRepository<Order> orderRepository, IEventPublisher eventPublisher, ILogger<OrderService> logger)
    {
        this.orderRepository = orderRepository;
        this.eventPublisher = eventPublisher;
        this.logger = logger;
    }

    public async Task<OrderDetailResponse> CreateOrderAsync(CreateOrderRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName))
        {
            throw new ValidationException("CustomerName is required.");
        }

        if (request.Items is null || request.Items.Count == 0)
        {
            throw new ValidationException("At least one order item is required.");
        }

        if (request.Items.Any(i => i.Quantity <= 0))
        {
            throw new ValidationException("Every item quantity must be greater than zero.");
        }

        if (request.Items.Any(i => i.UnitPrice < 0))
        {
            throw new ValidationException("Item unit price cannot be negative.");
        }

        var order = new Order
        {
            OrderNumber = "temporaryNumber",
            CustomerName = request.CustomerName.Trim(),
            CustomerEmail = request.CustomerEmail,
            OrderDate = DateTimeOffset.UtcNow,
            Status = OrderStatus.Pending,
            OrderItems = request.Items
                .Select(i => new OrderItem
                {
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                })
                .ToList()
        };

        order.TotalAmount = order.OrderItems.Sum(i => i.Quantity * i.UnitPrice);

        orderRepository.Create(order);
        await orderRepository.SaveChangesAsync();

        // OrderNumber needs that Id, so it's a second save.
        order.OrderNumber = $"ORD-{order.Id:D3}";
        await orderRepository.SaveChangesAsync();
        
        try
        {
            eventPublisher.Publish(new OrderCreatedEvent(order.Id, order.OrderNumber));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish OrderCreatedEvent for order {OrderNumber}.", order.OrderNumber);
        }

        return order.ToDetailResponse();
    }

    public async Task<List<OrderListItemResponse>> GetOrdersAsync(OrderStatus? status, DateTime? from, DateTime? to)
    {
        var query = orderRepository.Query().AsNoTracking();

        if (status is not null)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        if (from is not null)
        {
            var fromUtc = new DateTimeOffset(DateTime.SpecifyKind(from.Value.Date, DateTimeKind.Utc));
            query = query.Where(o => o.OrderDate >= fromUtc);
        }

        if (to is not null)
        {
            // Inclusive of the whole "to" day.
            var toUtcExclusive = new DateTimeOffset(DateTime.SpecifyKind(to.Value.Date, DateTimeKind.Utc)).AddDays(1);
            query = query.Where(o => o.OrderDate < toUtcExclusive);
        }

        var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();
        return orders.Select(o => o.ToListItemResponse()).ToList();
    }

    public async Task<OrderDetailResponse?> GetOrderByIdAsync(int id)
    {
        var order = await orderRepository.Query()
            .Include(o => o.OrderItems)
            .Include(o => o.InventoryChecks)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id);

        return order?.ToDetailResponse();
    }

    public async Task<(bool Success, bool NotFound, string? Error)> UpdateStatusAsync(int id, OrderStatus newStatus)
    {
        var order = await orderRepository.GetByIdAsync(id);
        if (order is null)
        {
            return (false, true, "Order not found.");
        }

        if (!OrderStatusHelper.IsValidTransition(order.Status, newStatus))
        {
            return (false, false, $"Cannot transition order from {order.Status} to {newStatus}.");
        }

        order.Status = newStatus;
        orderRepository.Update(order);
        await orderRepository.SaveChangesAsync();

        return (true, false, null);
    }
}
