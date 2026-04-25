using SapInspiredOrderManagement.Models;

namespace SapInspiredOrderManagement.Services;

public class StatusWorkflowService
{
    private static readonly IReadOnlyDictionary<OrderStatus, OrderStatus[]> Transitions =
        new Dictionary<OrderStatus, OrderStatus[]>
        {
            [OrderStatus.Draft] = new[] { OrderStatus.Submitted, OrderStatus.Cancelled },
            [OrderStatus.Submitted] = new[] { OrderStatus.Approved, OrderStatus.Cancelled },
            [OrderStatus.Approved] = new[] { OrderStatus.InFulfillment, OrderStatus.Cancelled },
            [OrderStatus.InFulfillment] = new[] { OrderStatus.Delivered, OrderStatus.Cancelled },
            [OrderStatus.Delivered] = new[] { OrderStatus.Invoiced },
            [OrderStatus.Invoiced] = new[] { OrderStatus.Closed },
            [OrderStatus.Closed] = Array.Empty<OrderStatus>(),
            [OrderStatus.Cancelled] = Array.Empty<OrderStatus>()
        };

    public IReadOnlyCollection<OrderStatus> GetAvailableTransitions(OrderStatus status, UserRole role)
    {
        if (!Transitions.TryGetValue(status, out var transitions))
        {
            return Array.Empty<OrderStatus>();
        }

        return transitions.Where(next => IsRoleAllowed(status, next, role)).ToArray();
    }

    public bool CanTransition(OrderStatus currentStatus, OrderStatus nextStatus, UserRole role)
    {
        return Transitions.TryGetValue(currentStatus, out var allowedStatuses)
            && allowedStatuses.Contains(nextStatus)
            && IsRoleAllowed(currentStatus, nextStatus, role);
    }

    public bool CanEditOrder(OrderStatus status, UserRole role)
    {
        return status == OrderStatus.Draft && role is UserRole.Admin or UserRole.SalesOperator;
    }

    private static bool IsRoleAllowed(OrderStatus currentStatus, OrderStatus nextStatus, UserRole role)
    {
        if (role == UserRole.Admin)
        {
            return true;
        }

        if (role == UserRole.SalesOperator)
        {
            return currentStatus == OrderStatus.Draft && nextStatus == OrderStatus.Submitted;
        }

        if (role == UserRole.Manager)
        {
            return currentStatus != OrderStatus.Draft;
        }

        return false;
    }
}
