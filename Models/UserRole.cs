using System.ComponentModel.DataAnnotations;

namespace SapInspiredOrderManagement.Models;

public enum UserRole
{
    Admin,

    [Display(Name = "Sales Operator")]
    SalesOperator,

    Manager
}
