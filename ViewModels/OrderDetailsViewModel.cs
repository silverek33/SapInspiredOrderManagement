using SapInspiredOrderManagement.Models;

namespace SapInspiredOrderManagement.ViewModels;

public class OrderDetailsViewModel
{
    public SalesOrder Order { get; set; } = null!;

    public IReadOnlyCollection<OrderStatus> AvailableTransitions { get; set; } = Array.Empty<OrderStatus>();

    public bool CanEdit { get; set; }
}
