using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SapInspiredOrderManagement.Data;
using SapInspiredOrderManagement.Extensions;
using SapInspiredOrderManagement.Models;
using SapInspiredOrderManagement.Services;
using SapInspiredOrderManagement.ViewModels;

namespace SapInspiredOrderManagement.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly OrderService _orderService;
    private readonly StatusWorkflowService _workflow;

    public OrdersController(ApplicationDbContext context, OrderService orderService, StatusWorkflowService workflow)
    {
        _context = context;
        _orderService = orderService;
        _workflow = workflow;
    }

    public async Task<IActionResult> Index(string? status, int? customerId, string? priority, DateTime? dateFrom, DateTime? dateTo)
    {
        var query = _context.SalesOrders
            .Include(order => order.Customer)
            .Include(order => order.CreatedByUser)
            .AsQueryable();

        if (Enum.TryParse<OrderStatus>(status, out var statusValue))
        {
            query = query.Where(order => order.Status == statusValue);
        }

        if (Enum.TryParse<OrderPriority>(priority, out var priorityValue))
        {
            query = query.Where(order => order.Priority == priorityValue);
        }

        if (customerId.HasValue)
        {
            query = query.Where(order => order.CustomerId == customerId.Value);
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(order => order.OrderDate >= dateFrom.Value);
        }

        if (dateTo.HasValue)
        {
            query = query.Where(order => order.OrderDate <= dateTo.Value);
        }

        var model = new OrderListViewModel
        {
            Orders = await query.OrderByDescending(order => order.CreatedAt).ToListAsync(),
            Status = status,
            CustomerId = customerId,
            Priority = priority,
            DateFrom = dateFrom,
            DateTo = dateTo,
            Customers = await BuildCustomerSelectListAsync()
        };

        return View(model);
    }

    [Authorize(Roles = "Admin,SalesOperator")]
    public async Task<IActionResult> Create()
    {
        var model = new CreateOrderViewModel();
        await PopulateLookupsAsync(model);
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SalesOperator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateOrderViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(model);
            return View(model);
        }

        var result = await _orderService.CreateOrderAsync(model, User.GetUserId());
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            await PopulateLookupsAsync(model);
            return View(model);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Details), new { id = result.Order!.SalesOrderId });
    }

    [Authorize(Roles = "Admin,SalesOperator")]
    public async Task<IActionResult> Edit(int id)
    {
        var order = await _context.SalesOrders
            .Include(item => item.Items)
            .SingleOrDefaultAsync(item => item.SalesOrderId == id);

        if (order is null)
        {
            return NotFound();
        }

        if (!_workflow.CanEditOrder(order.Status, User.GetUserRole()))
        {
            TempData["Error"] = "Only Draft orders can be edited by this role.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var model = new CreateOrderViewModel
        {
            CustomerId = order.CustomerId,
            OrderDate = order.OrderDate,
            RequestedDeliveryDate = order.RequestedDeliveryDate,
            Priority = order.Priority,
            Notes = order.Notes,
            Items = order.Items
                .Select(item => new OrderItemInputModel { ProductId = item.ProductId, Quantity = item.Quantity })
                .Concat(Enumerable.Range(0, 3).Select(_ => new OrderItemInputModel()))
                .ToList()
        };

        await PopulateLookupsAsync(model);
        ViewBag.OrderId = id;
        ViewBag.OrderNumber = order.OrderNumber;
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SalesOperator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateOrderViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(model);
            ViewBag.OrderId = id;
            return View(model);
        }

        var result = await _orderService.UpdateDraftAsync(id, model, User.GetUserId(), User.GetUserRole());
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            await PopulateLookupsAsync(model);
            ViewBag.OrderId = id;
            return View(model);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _context.SalesOrders
            .Include(item => item.Customer)
            .Include(item => item.CreatedByUser)
            .Include(item => item.ApprovedByUser)
            .Include(item => item.Items)
                .ThenInclude(item => item.Product)
            .Include(item => item.StatusHistories)
                .ThenInclude(item => item.ChangedByUser)
            .AsSplitQuery()
            .SingleOrDefaultAsync(item => item.SalesOrderId == id);

        if (order is null)
        {
            return NotFound();
        }

        var role = User.GetUserRole();
        var model = new OrderDetailsViewModel
        {
            Order = order,
            AvailableTransitions = _workflow.GetAvailableTransitions(order.Status, role),
            CanEdit = _workflow.CanEditOrder(order.Status, role)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, OrderStatus nextStatus, string? comment)
    {
        var result = await _orderService.ChangeStatusAsync(id, nextStatus, User.GetUserId(), User.GetUserRole(), comment);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Approval()
    {
        var orders = await _context.SalesOrders
            .Include(item => item.Customer)
            .Include(item => item.CreatedByUser)
            .Where(item => item.Status == OrderStatus.Submitted)
            .OrderBy(item => item.RequestedDeliveryDate)
            .ToListAsync();

        return View(orders);
    }

    private async Task PopulateLookupsAsync(CreateOrderViewModel model)
    {
        model.Customers = await BuildCustomerSelectListAsync();
        model.Products = await _context.Products
            .Where(product => product.IsActive)
            .OrderBy(product => product.ProductCode)
            .Select(product => new SelectListItem
            {
                Value = product.ProductId.ToString(),
                Text = $"{product.ProductCode} - {product.Name} ({product.UnitPrice:N2} {product.Currency})"
            })
            .ToListAsync();
    }

    private async Task<IEnumerable<SelectListItem>> BuildCustomerSelectListAsync()
    {
        return await _context.Customers
            .Where(customer => customer.IsActive)
            .OrderBy(customer => customer.CustomerCode)
            .Select(customer => new SelectListItem
            {
                Value = customer.CustomerId.ToString(),
                Text = $"{customer.CustomerCode} - {customer.Name}"
            })
            .ToListAsync();
    }
}
