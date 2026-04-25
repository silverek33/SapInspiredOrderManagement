using System.ComponentModel.DataAnnotations;

namespace SapInspiredOrderManagement.ViewModels;

public class OrderItemInputModel
{
    public int? ProductId { get; set; }

    [Range(1, 999999)]
    public int Quantity { get; set; } = 1;
}
