using SapInspiredOrderManagement.Models;

namespace SapInspiredOrderManagement.ViewModels;

public class DashboardViewModel
{
    public IDictionary<OrderStatus, int> StatusCounts { get; set; } = new Dictionary<OrderStatus, int>();

    public IReadOnlyList<SalesOrder> RecentOrders { get; set; } = Array.Empty<SalesOrder>();

    public IReadOnlyList<SalesOrder> HighPriorityOrders { get; set; } = Array.Empty<SalesOrder>();

    public int PendingApprovals { get; set; }

    public decimal OpenOrderValue { get; set; }
}
