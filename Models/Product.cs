using System.ComponentModel.DataAnnotations;

namespace SapInspiredOrderManagement.Models;

public class Product
{
    public int ProductId { get; set; }

    [Required]
    [StringLength(24)]
    [Display(Name = "Product Code")]
    public string ProductCode { get; set; } = string.Empty;

    [Required]
    [StringLength(160)]
    public string Name { get; set; } = string.Empty;

    [StringLength(80)]
    public string? Category { get; set; }

    [Range(0, 999999)]
    [Display(Name = "Unit Price")]
    public decimal UnitPrice { get; set; }

    [Required]
    [StringLength(3)]
    public string Currency { get; set; } = "EUR";

    [Range(0, 999999)]
    [Display(Name = "Stock Quantity")]
    public int StockQuantity { get; set; }

    [Display(Name = "Available for new orders")]
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<SalesOrderItem> SalesOrderItems { get; set; } = new List<SalesOrderItem>();
}
