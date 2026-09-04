using OPS.Domain.Entities;

namespace OPS.Application.Common.Helpers;

public static class OrderStatusHelper
{
    /// <summary>
    /// Pending    => Processing, Canceled
    /// Processing => Completed, Failed, Canceled
    /// Completed / Failed / Canceled => (terminal, nothing allowed)
    /// </summary>
    public static bool IsValidTransition(OrderStatus current, OrderStatus next) => current switch
    {
        OrderStatus.Pending => next is OrderStatus.Processing or OrderStatus.Cancelled,
        OrderStatus.Processing => next is OrderStatus.Completed or OrderStatus.Failed or OrderStatus.Cancelled,
        _ => false
    };
}