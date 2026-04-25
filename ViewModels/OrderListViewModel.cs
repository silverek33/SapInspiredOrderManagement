using Microsoft.AspNetCore.Mvc.Rendering;
using SapInspiredOrderManagement.Models;

namespace SapInspiredOrderManagement.ViewModels;

public class OrderListViewModel
{
    public IReadOnlyList<SalesOrder> Orders { get; set; } = Array.Empty<SalesOrder>();

    public string? Status { get; set; }

    public int? CustomerId { get; set; }

    public string? Priority { get; set; }

    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }

    public IEnumerable<SelectListItem> Customers { get; set; } = Enumerable.Empty<SelectListItem>();
}
