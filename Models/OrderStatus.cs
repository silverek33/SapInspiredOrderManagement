using System.ComponentModel.DataAnnotations;

namespace SapInspiredOrderManagement.Models;

public enum OrderStatus
{
    Draft,
    Submitted,
    Approved,

    [Display(Name = "In Fulfillment")]
    InFulfillment,

    Delivered,
    Invoiced,
    Closed,
    Cancelled
}
