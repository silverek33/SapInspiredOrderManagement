using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SapInspiredOrderManagement.Data;
using SapInspiredOrderManagement.Models;
using SapInspiredOrderManagement.ViewModels;

namespace SapInspiredOrderManagement.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var statusCounts = await _context.SalesOrders
            .GroupBy(order => order.Status)
            .ToDictionaryAsync(group => group.Key, group => group.Count());

        var recentOrders = await _context.SalesOrders
            .Include(order => order.Customer)
            .Include(order => order.CreatedByUser)
            .OrderByDescending(order => order.CreatedAt)
            .Take(6)
            .ToListAsync();

        var highPriorityOrders = await _context.SalesOrders
            .Include(order => order.Customer)
            .Where(order => (order.Priority == OrderPriority.High || order.Priority == OrderPriority.Critical)
                && order.Status != OrderStatus.Closed
                && order.Status != OrderStatus.Cancelled)
            .OrderBy(order => order.RequestedDeliveryDate)
            .Take(5)
            .ToListAsync();

        var openOrderValue = await _context.SalesOrders
            .Where(order => order.Status != OrderStatus.Closed && order.Status != OrderStatus.Cancelled)
            .SumAsync(order => order.TotalAmount);

        var model = new DashboardViewModel
        {
            StatusCounts = statusCounts,
            RecentOrders = recentOrders,
            HighPriorityOrders = highPriorityOrders,
            PendingApprovals = statusCounts.GetValueOrDefault(OrderStatus.Submitted),
            OpenOrderValue = openOrderValue
        };

        return View(model);
    }
}
