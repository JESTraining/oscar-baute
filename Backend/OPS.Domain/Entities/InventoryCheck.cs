using OPS.Domain.Common;

namespace OPS.Domain.Entities;

public class InventoryCheck : BaseEntity
{
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public DateTimeOffset CheckDate { get; set; }
    public bool IsAvailable { get; set; }
}