using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SapInspiredOrderManagement.Data;
using SapInspiredOrderManagement.Models;

namespace SapInspiredOrderManagement.Controllers;

[Authorize]
public class CustomersController : Controller
{
    private readonly ApplicationDbContext _context;

    public CustomersController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search, bool includeInactive = false)
    {
        var query = _context.Customers.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(customer => customer.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(customer =>
                customer.Name.Contains(search)
                || customer.CustomerCode.Contains(search)
                || (customer.TaxId != null && customer.TaxId.Contains(search)));
        }

        ViewBag.Search = search;
        ViewBag.IncludeInactive = includeInactive;
        return View(await query.OrderBy(customer => customer.CustomerCode).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var customer = await _context.Customers
            .Include(item => item.SalesOrders)
            .SingleOrDefaultAsync(item => item.CustomerId == id);

        return customer is null ? NotFound() : View(customer);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View(new Customer());
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Customer customer)
    {
        if (await _context.Customers.AnyAsync(item => item.CustomerCode == customer.CustomerCode))
        {
            ModelState.AddModelError(nameof(Customer.CustomerCode), "Customer code already exists.");
        }

        if (!ModelState.IsValid)
        {
            return View(customer);
        }

        customer.CreatedAt = DateTime.UtcNow;
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Customer created.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        return customer is null ? NotFound() : View(customer);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Customer customer)
    {
        if (id != customer.CustomerId)
        {
            return BadRequest();
        }

        if (await _context.Customers.AnyAsync(item => item.CustomerCode == customer.CustomerCode && item.CustomerId != id))
        {
            ModelState.AddModelError(nameof(Customer.CustomerCode), "Customer code already exists.");
        }

        if (!ModelState.IsValid)
        {
            return View(customer);
        }

        _context.Update(customer);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Customer updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer is null)
        {
            return NotFound();
        }

        customer.IsActive = !customer.IsActive;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { includeInactive = true });
    }
}
