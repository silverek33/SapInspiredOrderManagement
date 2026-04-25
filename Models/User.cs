using System.ComponentModel.DataAnnotations;

namespace SapInspiredOrderManagement.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(160)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<SalesOrder> CreatedOrders { get; set; } = new List<SalesOrder>();

    public ICollection<SalesOrder> ApprovedOrders { get; set; } = new List<SalesOrder>();

    public ICollection<StatusHistory> StatusHistories { get; set; } = new List<StatusHistory>();
}
