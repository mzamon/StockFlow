using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StockFlow.Models;
namespace StockFlow.Data;
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Product> Products => Set<Product>(); public DbSet<Supplier> Suppliers => Set<Supplier>(); public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>(); public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>(); public DbSet<Sale> Sales => Set<Sale>(); public DbSet<SaleLine> SaleLines => Set<SaleLine>(); public DbSet<StockMovement> StockMovements => Set<StockMovement>(); public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Product>().HasIndex(x => x.Sku).IsUnique();
        b.Entity<Product>().HasIndex(x => x.Barcode).IsUnique().HasFilter("[Barcode] IS NOT NULL");
        b.Entity<Product>().Property(x => x.CostPrice).HasPrecision(18, 2);
        b.Entity<Product>().Property(x => x.SellingPrice).HasPrecision(18, 2);
        b.Entity<PurchaseOrder>().HasIndex(x => x.OrderNumber).IsUnique();
        b.Entity<PurchaseOrderLine>().Property(x => x.UnitCost).HasPrecision(18, 2);
        b.Entity<Sale>().HasIndex(x => x.SaleNumber).IsUnique();
        b.Entity<Sale>().Property(x => x.Total).HasPrecision(18, 2);
        b.Entity<SaleLine>().Property(x => x.UnitPrice).HasPrecision(18, 2);
        b.Entity<PurchaseOrderLine>()
            .HasOne(x => x.PurchaseOrder)
            .WithMany(x => x.Lines)
            .HasForeignKey(x => x.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Entity<SaleLine>()
            .HasOne(x => x.Sale)
            .WithMany(x => x.Lines)
            .HasForeignKey(x => x.SaleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
