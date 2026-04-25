using Microsoft.EntityFrameworkCore;
using SapInspiredOrderManagement.Models;

namespace SapInspiredOrderManagement.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();

    public DbSet<SalesOrderItem> SalesOrderItems => Set<SalesOrderItem>();

    public DbSet<StatusHistory> StatusHistories => Set<StatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(user => user.Email).IsUnique();
            entity.Property(user => user.Role).HasConversion<string>().HasMaxLength(32);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasIndex(customer => customer.CustomerCode).IsUnique();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(product => product.ProductCode).IsUnique();
            entity.Property(product => product.UnitPrice).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<SalesOrder>(entity =>
        {
            entity.HasIndex(order => order.OrderNumber).IsUnique();
            entity.Property(order => order.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(order => order.Priority).HasConversion<string>().HasMaxLength(32);
            entity.Property(order => order.TotalAmount).HasColumnType("decimal(18,2)");

            entity.HasOne(order => order.Customer)
                .WithMany(customer => customer.SalesOrders)
                .HasForeignKey(order => order.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(order => order.CreatedByUser)
                .WithMany(user => user.CreatedOrders)
                .HasForeignKey(order => order.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(order => order.ApprovedByUser)
                .WithMany(user => user.ApprovedOrders)
                .HasForeignKey(order => order.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SalesOrderItem>(entity =>
        {
            entity.Property(item => item.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(item => item.LineTotal).HasColumnType("decimal(18,2)");

            entity.HasOne(item => item.SalesOrder)
                .WithMany(order => order.Items)
                .HasForeignKey(item => item.SalesOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(item => item.Product)
                .WithMany(product => product.SalesOrderItems)
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<StatusHistory>(entity =>
        {
            entity.Property(history => history.OldStatus).HasConversion<string>().HasMaxLength(32);
            entity.Property(history => history.NewStatus).HasConversion<string>().HasMaxLength(32);

            entity.HasOne(history => history.SalesOrder)
                .WithMany(order => order.StatusHistories)
                .HasForeignKey(history => history.SalesOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(history => history.ChangedByUser)
                .WithMany(user => user.StatusHistories)
                .HasForeignKey(history => history.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
