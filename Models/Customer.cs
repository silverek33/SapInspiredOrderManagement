using System.ComponentModel.DataAnnotations;

namespace SapInspiredOrderManagement.Models;

public class Customer
{
    public int CustomerId { get; set; }

    [Required]
    [StringLength(24)]
    [Display(Name = "Customer Code")]
    public string CustomerCode { get; set; } = string.Empty;

    [Required]
    [StringLength(160)]
    public string Name { get; set; } = string.Empty;

    [StringLength(40)]
    [Display(Name = "Tax ID")]
    public string? TaxId { get; set; }

    [EmailAddress]
    [StringLength(160)]
    public string? Email { get; set; }

    [Phone]
    [StringLength(40)]
    public string? Phone { get; set; }

    [StringLength(240)]
    public string? Address { get; set; }

    [StringLength(80)]
    public string? Country { get; set; }

    [Display(Name = "Available for new orders")]
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
}
