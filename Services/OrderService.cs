using Microsoft.EntityFrameworkCore;
using SapInspiredOrderManagement.Data;
using SapInspiredOrderManagement.Models;
using SapInspiredOrderManagement.ViewModels;

namespace SapInspiredOrderManagement.Services;

public class OrderService
{
    private readonly ApplicationDbContext _context;
    private readonly StatusWorkflowService _workflow;

    public OrderService(ApplicationDbContext context, StatusWorkflowService workflow)
    {
        _context = context;
        _workflow = workflow;
    }

    public async Task<(bool Success, string Message, SalesOrder? Order)> CreateOrderAsync(CreateOrderViewModel model, int userId)
    {
        var customer = await _context.Customers.FindAsync(model.CustomerId);
        if (customer is null || !customer.IsActive)
        {
            return (false, "Select an active customer.", null);
        }

        var itemInputs = model.Items.Where(item => item.ProductId.HasValue).ToList();
        if (!itemInputs.Any())
        {
            return (false, "Order must contain at least one item.", null);
        }

        var orderItems = new List<SalesOrderItem>();
        foreach (var input in itemInputs)
        {
            if (input.Quantity <= 0)
            {
                return (false, "Quantity must be greater than zero.", null);
            }

            var product = await _context.Products.FindAsync(input.ProductId!.Value);
            if (product is null || !product.IsActive)
            {
                return (false, "Only active products can be added to new orders.", null);
            }

            var lineTotal = product.UnitPrice * input.Quantity;
            orderItems.Add(new SalesOrderItem
            {
                ProductId = product.ProductId,
                Quantity = input.Quantity,
                UnitPrice = product.UnitPrice,
                LineTotal = lineTotal
            });
        }

        var order = new SalesOrder
        {
            OrderNumber = await GenerateOrderNumberAsync(),
            CustomerId = model.CustomerId,
            OrderDate = model.OrderDate,
            RequestedDeliveryDate = model.RequestedDeliveryDate,
            Priority = model.Priority,
            Status = OrderStatus.Draft,
            CreatedByUserId = userId,
            Notes = model.Notes,
            Items = orderItems,
            TotalAmount = orderItems.Sum(item => item.LineTotal),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.SalesOrders.Add(order);
        await _context.SaveChangesAsync();

        _context.StatusHistories.Add(new StatusHistory
        {
            SalesOrderId = order.SalesOrderId,
            OldStatus = null,
            NewStatus = OrderStatus.Draft,
            ChangedByUserId = userId,
            Comment = "Order created.",
            ChangedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return (true, "Order created as Draft.", order);
    }

    public async Task<(bool Success, string Message)> UpdateDraftAsync(int orderId, CreateOrderViewModel model, int userId, UserRole role)
    {
        var order = await _context.SalesOrders
            .Include(salesOrder => salesOrder.Items)
            .SingleOrDefaultAsync(salesOrder => salesOrder.SalesOrderId == orderId);

        if (order is null)
        {
            return (false, "Order not found.");
        }

        if (!_workflow.CanEditOrder(order.Status, role))
        {
            return (false, "Only Draft orders can be edited by this role.");
        }

        var customer = await _context.Customers.FindAsync(model.CustomerId);
        if (customer is null || !customer.IsActive)
        {
            return (false, "Select an active customer.");
        }

        var itemInputs = model.Items.Where(item => item.ProductId.HasValue).ToList();
        if (!itemInputs.Any())
        {
            return (false, "Order must contain at least one item.");
        }

        var newItems = new List<SalesOrderItem>();
        foreach (var input in itemInputs)
        {
            if (input.Quantity <= 0)
            {
                return (false, "Quantity must be greater than zero.");
            }

            var product = await _context.Products.FindAsync(input.ProductId!.Value);
            if (product is null || !product.IsActive)
            {
                return (false, "Only active products can be added to orders.");
            }

            newItems.Add(new SalesOrderItem
            {
                SalesOrderId = order.SalesOrderId,
                ProductId = product.ProductId,
                Quantity = input.Quantity,
                UnitPrice = product.UnitPrice,
                LineTotal = product.UnitPrice * input.Quantity
            });
        }

        _context.SalesOrderItems.RemoveRange(order.Items);
        order.CustomerId = model.CustomerId;
        order.OrderDate = model.OrderDate;
        order.RequestedDeliveryDate = model.RequestedDeliveryDate;
        order.Priority = model.Priority;
        order.Notes = model.Notes;
        order.Items = newItems;
        order.TotalAmount = newItems.Sum(item => item.LineTotal);
        order.UpdatedAt = DateTime.UtcNow;

        _context.StatusHistories.Add(new StatusHistory
        {
            SalesOrderId = order.SalesOrderId,
            OldStatus = order.Status,
            NewStatus = order.Status,
            ChangedByUserId = userId,
            Comment = "Draft details updated.",
            ChangedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return (true, "Draft order updated.");
    }

    public async Task<(bool Success, string Message)> ChangeStatusAsync(int orderId, OrderStatus nextStatus, int userId, UserRole role, string? comment)
    {
        var order = await _context.SalesOrders
            .Include(salesOrder => salesOrder.Items)
            .SingleOrDefaultAsync(salesOrder => salesOrder.SalesOrderId == orderId);

        if (order is null)
        {
            return (false, "Order not found.");
        }

        if (order.Status is OrderStatus.Cancelled or OrderStatus.Closed)
        {
            return (false, "Cancelled and Closed orders cannot be changed.");
        }

        if (order.Status == OrderStatus.Draft && !order.Items.Any())
        {
            return (false, "Draft order must contain at least one item before submission.");
        }

        if (!_workflow.CanTransition(order.Status, nextStatus, role))
        {
            return (false, $"Transition from {order.Status.GetDisplayName()} to {nextStatus.GetDisplayName()} is not allowed for your role.");
        }

        var oldStatus = order.Status;
        order.Status = nextStatus;
        order.UpdatedAt = DateTime.UtcNow;

        if (nextStatus == OrderStatus.Approved)
        {
            order.ApprovedByUserId = userId;
        }

        _context.StatusHistories.Add(new StatusHistory
        {
            SalesOrderId = order.SalesOrderId,
            OldStatus = oldStatus,
            NewStatus = nextStatus,
            ChangedByUserId = userId,
            ChangedAt = DateTime.UtcNow,
            Comment = string.IsNullOrWhiteSpace(comment)
                ? $"Status changed to {nextStatus.GetDisplayName()}."
                : comment.Trim()
        });

        await _context.SaveChangesAsync();
        return (true, $"Order moved to {nextStatus.GetDisplayName()}.");
    }

    private async Task<string> GenerateOrderNumberAsync()
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var count = await _context.SalesOrders.CountAsync(order => order.CreatedAt >= today && order.CreatedAt < tomorrow);
        return $"SO-{datePart}-{count + 1:000}";
    }
}
