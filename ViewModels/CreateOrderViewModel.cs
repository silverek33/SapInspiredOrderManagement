using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using SapInspiredOrderManagement.Models;

namespace SapInspiredOrderManagement.ViewModels;

public class CreateOrderViewModel
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Select a customer.")]
    [Display(Name = "Customer")]
    public int CustomerId { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Order Date")]
    public DateTime OrderDate { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    [Display(Name = "Requested Delivery Date")]
    public DateTime? RequestedDeliveryDate { get; set; }

    public OrderPriority Priority { get; set; } = OrderPriority.Normal;

    [StringLength(500)]
    public string? Notes { get; set; }

    public List<OrderItemInputModel> Items { get; set; } = new()
    {
        new OrderItemInputModel(),
        new OrderItemInputModel(),
        new OrderItemInputModel()
    };

    [ValidateNever]
    public IEnumerable<SelectListItem> Customers { get; set; } = Enumerable.Empty<SelectListItem>();

    [ValidateNever]
    public IEnumerable<SelectListItem> Products { get; set; } = Enumerable.Empty<SelectListItem>();
}
