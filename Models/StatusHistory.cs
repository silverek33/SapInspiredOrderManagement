using System.ComponentModel.DataAnnotations;

namespace SapInspiredOrderManagement.Models;

public class StatusHistory
{
    public int StatusHistoryId { get; set; }

    public int SalesOrderId { get; set; }

    public SalesOrder SalesOrder { get; set; } = null!;

    [Display(Name = "Old Status")]
    public OrderStatus? OldStatus { get; set; }

    [Display(Name = "New Status")]
    public OrderStatus NewStatus { get; set; }

    [Display(Name = "Changed By")]
    public int ChangedByUserId { get; set; }

    public User ChangedByUser { get; set; } = null!;

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    [StringLength(500)]
    public string? Comment { get; set; }
}
