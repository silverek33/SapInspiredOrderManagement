using System.Security.Claims;
using SapInspiredOrderManagement.Models;

namespace SapInspiredOrderManagement.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : 0;
    }

    public static UserRole GetUserRole(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.Role);
        return Enum.TryParse<UserRole>(value, out var role) ? role : UserRole.SalesOperator;
    }
}
