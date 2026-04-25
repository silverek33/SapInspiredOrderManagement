using System.ComponentModel.DataAnnotations;

namespace SapInspiredOrderManagement.Models;

public class SalesOrderItem
{
    public int SalesOrderItemId { get; set; }

    public int SalesOrderId { get; set; }

    public SalesOrder SalesOrder { get; set; } = null!;

    [Display(Name = "Product")]
    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    [Range(1, 999999)]
    public int Quantity { get; set; }

    [Display(Name = "Unit Price")]
    public decimal UnitPrice { get; set; }

    [Display(Name = "Line Total")]
    public decimal LineTotal { get; set; }
}
