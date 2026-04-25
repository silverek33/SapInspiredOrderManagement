using System.ComponentModel.DataAnnotations;

namespace SapInspiredOrderManagement.Models;

public class SalesOrder
{
    public int SalesOrderId { get; set; }

    [Required]
    [StringLength(32)]
    [Display(Name = "Order Number")]
    public string OrderNumber { get; set; } = string.Empty;

    [Display(Name = "Customer")]
    public int CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;

    [DataType(DataType.Date)]
    [Display(Name = "Order Date")]
    public DateTime OrderDate { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    [Display(Name = "Requested Delivery Date")]
    public DateTime? RequestedDeliveryDate { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Draft;

    public OrderPriority Priority { get; set; } = OrderPriority.Normal;

    [Display(Name = "Created By")]
    public int CreatedByUserId { get; set; }

    public User CreatedByUser { get; set; } = null!;

    [Display(Name = "Approved By")]
    public int? ApprovedByUserId { get; set; }

    public User? ApprovedByUser { get; set; }

    [Display(Name = "Total Amount")]
    public decimal TotalAmount { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<SalesOrderItem> Items { get; set; } = new List<SalesOrderItem>();

    public ICollection<StatusHistory> StatusHistories { get; set; } = new List<StatusHistory>();
}
