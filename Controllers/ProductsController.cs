using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SapInspiredOrderManagement.Data;
using SapInspiredOrderManagement.Models;

namespace SapInspiredOrderManagement.Controllers;

[Authorize]
public class ProductsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search, bool includeInactive = false)
    {
        var query = _context.Products.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(product => product.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(product =>
                product.Name.Contains(search)
                || product.ProductCode.Contains(search)
                || (product.Category != null && product.Category.Contains(search)));
        }

        ViewBag.Search = search;
        ViewBag.IncludeInactive = includeInactive;
        return View(await query.OrderBy(product => product.ProductCode).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _context.Products
            .Include(item => item.SalesOrderItems)
            .SingleOrDefaultAsync(item => item.ProductId == id);

        return product is null ? NotFound() : View(product);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View(new Product { Currency = "EUR" });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        if (await _context.Products.AnyAsync(item => item.ProductCode == product.ProductCode))
        {
            ModelState.AddModelError(nameof(Product.ProductCode), "Product code already exists.");
        }

        if (!ModelState.IsValid)
        {
            return View(product);
        }

        product.CreatedAt = DateTime.UtcNow;
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Product created.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _context.Products.FindAsync(id);
        return product is null ? NotFound() : View(product);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product)
    {
        if (id != product.ProductId)
        {
            return BadRequest();
        }

        if (await _context.Products.AnyAsync(item => item.ProductCode == product.ProductCode && item.ProductId != id))
        {
            ModelState.AddModelError(nameof(Product.ProductCode), "Product code already exists.");
        }

        if (!ModelState.IsValid)
        {
            return View(product);
        }

        _context.Update(product);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Product updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        product.IsActive = !product.IsActive;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { includeInactive = true });
    }
}
