using OPS.Application.Dtos;
using OPS.Domain.Entities;

namespace OPS.Application.Interfaces;
/// <summary>
/// Defines the contract for order-related operations, including creating orders, retrieving orders, and updating order statuses.
/// </summary>
public interface IOrderService
{
    Task<OrderDetailResponse> CreateOrderAsync(CreateOrderRequest request);
    Task<List<OrderListItemResponse>> GetOrdersAsync(OrderStatus? status, DateTime? from, DateTime? to);
    Task<OrderDetailResponse?> GetOrderByIdAsync(int id);
    Task<(bool Success, bool NotFound, string? Error)> UpdateStatusAsync(int id, OrderStatus newStatus);
}