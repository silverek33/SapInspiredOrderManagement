using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SapInspiredOrderManagement.Models;

namespace SapInspiredOrderManagement.Data;

public static class SeedData
{
    public static void Initialize(ApplicationDbContext context)
    {
        if (!context.Users.Any())
        {
            var hasher = new PasswordHasher<User>();
            var users = new[]
            {
                CreateUser(hasher, "System Administrator", "admin@sapdemo.local", UserRole.Admin, "Admin123!"),
                CreateUser(hasher, "Sarah Sales", "sales@sapdemo.local", UserRole.SalesOperator, "Sales123!"),
                CreateUser(hasher, "Mark Manager", "manager@sapdemo.local", UserRole.Manager, "Manager123!")
            };

            context.Users.AddRange(users);
            context.SaveChanges();
        }

        if (!context.Customers.Any())
        {
            context.Customers.AddRange(
                new Customer
                {
                    CustomerCode = "CUST-1001",
                    Name = "Northwind Manufacturing",
                    TaxId = "PL5210000001",
                    Email = "orders@northwind.example",
                    Phone = "+48 22 100 10 01",
                    Address = "Business Park 12, Warsaw",
                    Country = "Poland"
                },
                new Customer
                {
                    CustomerCode = "CUST-1002",
                    Name = "Contoso Retail Group",
                    TaxId = "DE129000000",
                    Email = "procurement@contoso.example",
                    Phone = "+49 30 200 20 02",
                    Address = "Alexanderplatz 4, Berlin",
                    Country = "Germany"
                },
                new Customer
                {
                    CustomerCode = "CUST-1003",
                    Name = "Fabrikam Logistics",
                    TaxId = "CZ8400000001",
                    Email = "sales@fabrikam.example",
                    Phone = "+420 2 300 30 03",
                    Address = "Dockside 8, Prague",
                    Country = "Czech Republic"
                });

            context.SaveChanges();
        }

        if (!context.Products.Any())
        {
            context.Products.AddRange(
                new Product
                {
                    ProductCode = "MAT-100",
                    Name = "Industrial Sensor",
                    Category = "Components",
                    UnitPrice = 129.00m,
                    Currency = "EUR",
                    StockQuantity = 180
                },
                new Product
                {
                    ProductCode = "MAT-200",
                    Name = "Control Cabinet",
                    Category = "Equipment",
                    UnitPrice = 890.00m,
                    Currency = "EUR",
                    StockQuantity = 24
                },
                new Product
                {
                    ProductCode = "MAT-300",
                    Name = "Maintenance Kit",
                    Category = "Service Parts",
                    UnitPrice = 74.50m,
                    Currency = "EUR",
                    StockQuantity = 320
                },
                new Product
                {
                    ProductCode = "SRV-900",
                    Name = "Implementation Support Day",
                    Category = "Service",
                    UnitPrice = 550.00m,
                    Currency = "EUR",
                    StockQuantity = 60
                });

            context.SaveChanges();
        }

        if (!context.SalesOrders.Any())
        {
            var salesUser = context.Users.Single(user => user.Email == "sales@sapdemo.local");
            var manager = context.Users.Single(user => user.Email == "manager@sapdemo.local");
            var customers = context.Customers.AsNoTracking().ToList();
            var products = context.Products.AsNoTracking().ToList();

            var submittedOrder = new SalesOrder
            {
                OrderNumber = "SO-20260411-001",
                CustomerId = customers[0].CustomerId,
                OrderDate = DateTime.Today.AddDays(-2),
                RequestedDeliveryDate = DateTime.Today.AddDays(7),
                Priority = OrderPriority.High,
                Status = OrderStatus.Submitted,
                CreatedByUserId = salesUser.Id,
                Notes = "Customer requested priority processing.",
                Items = new List<SalesOrderItem>
                {
                    BuildItem(products[0], 12),
                    BuildItem(products[2], 6)
                }
            };
            submittedOrder.TotalAmount = submittedOrder.Items.Sum(item => item.LineTotal);

            var approvedOrder = new SalesOrder
            {
                OrderNumber = "SO-20260411-002",
                CustomerId = customers[1].CustomerId,
                OrderDate = DateTime.Today.AddDays(-5),
                RequestedDeliveryDate = DateTime.Today.AddDays(3),
                Priority = OrderPriority.Normal,
                Status = OrderStatus.Approved,
                CreatedByUserId = salesUser.Id,
                ApprovedByUserId = manager.Id,
                Notes = "Standard replenishment order.",
                Items = new List<SalesOrderItem>
                {
                    BuildItem(products[1], 2),
                    BuildItem(products[3], 1)
                }
            };
            approvedOrder.TotalAmount = approvedOrder.Items.Sum(item => item.LineTotal);

            context.SalesOrders.AddRange(submittedOrder, approvedOrder);
            context.SaveChanges();

            context.StatusHistories.AddRange(
                new StatusHistory
                {
                    SalesOrderId = submittedOrder.SalesOrderId,
                    OldStatus = null,
                    NewStatus = OrderStatus.Draft,
                    ChangedByUserId = salesUser.Id,
                    ChangedAt = submittedOrder.CreatedAt,
                    Comment = "Order created."
                },
                new StatusHistory
                {
                    SalesOrderId = submittedOrder.SalesOrderId,
                    OldStatus = OrderStatus.Draft,
                    NewStatus = OrderStatus.Submitted,
                    ChangedByUserId = salesUser.Id,
                    ChangedAt = submittedOrder.CreatedAt.AddMinutes(15),
                    Comment = "Submitted for manager approval."
                },
                new StatusHistory
                {
                    SalesOrderId = approvedOrder.SalesOrderId,
                    OldStatus = null,
                    NewStatus = OrderStatus.Draft,
                    ChangedByUserId = salesUser.Id,
                    ChangedAt = approvedOrder.CreatedAt,
                    Comment = "Order created."
                },
                new StatusHistory
                {
                    SalesOrderId = approvedOrder.SalesOrderId,
                    OldStatus = OrderStatus.Draft,
                    NewStatus = OrderStatus.Submitted,
                    ChangedByUserId = salesUser.Id,
                    ChangedAt = approvedOrder.CreatedAt.AddMinutes(8),
                    Comment = "Submitted for manager approval."
                },
                new StatusHistory
                {
                    SalesOrderId = approvedOrder.SalesOrderId,
                    OldStatus = OrderStatus.Submitted,
                    NewStatus = OrderStatus.Approved,
                    ChangedByUserId = manager.Id,
                    ChangedAt = approvedOrder.CreatedAt.AddHours(2),
                    Comment = "Commercial terms approved."
                });

            context.SaveChanges();
        }
    }

    private static User CreateUser(PasswordHasher<User> hasher, string fullName, string email, UserRole role, string password)
    {
        var user = new User
        {
            FullName = fullName,
            Email = email,
            Role = role,
            IsActive = true
        };

        user.PasswordHash = hasher.HashPassword(user, password);
        return user;
    }

    private static SalesOrderItem BuildItem(Product product, int quantity)
    {
        return new SalesOrderItem
        {
            ProductId = product.ProductId,
            Quantity = quantity,
            UnitPrice = product.UnitPrice,
            LineTotal = quantity * product.UnitPrice
        };
    }
}
