using Microsoft.AspNetCore.Identity;
using StockFlow.Models;
using Microsoft.EntityFrameworkCore;
namespace StockFlow.Data;
public static class SeedData
{
    public static async Task Initialize(IServiceProvider sp)
    {
        var roles = sp.GetRequiredService<RoleManager<IdentityRole>>(); var users = sp.GetRequiredService<UserManager<ApplicationUser>>(); var db = sp.GetRequiredService<ApplicationDbContext>();
        foreach (var r in new[] { "Admin", "Manager", "StockClerk", "Procurement", "Viewer" }) if (!await roles.RoleExistsAsync(r)) await roles.CreateAsync(new IdentityRole(r));
        const string email = "admin@admin.com"; var admin = await users.FindByEmailAsync(email); if (admin == null) { admin = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, FullName = "StockFlow Administrator", IsActive = true }; var result = await users.CreateAsync(admin, "Password123!"); if (result.Succeeded) await users.AddToRoleAsync(admin, "Admin"); } else if (!await users.IsInRoleAsync(admin, "Admin")) await users.AddToRoleAsync(admin, "Admin");
        var extraDemoSkus = new[] { "SKU00005", "SKU00006", "SKU00007", "SKU00008" };
        var extraDemoProducts = await db.Products
            .Where(product => extraDemoSkus.Contains(product.Sku))
            .ToListAsync();

        if (extraDemoProducts.Count > 0)
        {
            var extraProductIds = extraDemoProducts.Select(product => product.Id).ToArray();
            var extraMovements = await db.StockMovements
                .Where(movement => extraProductIds.Contains(movement.ProductId))
                .ToListAsync();

            db.StockMovements.RemoveRange(extraMovements);
            db.Products.RemoveRange(extraDemoProducts);
            await db.SaveChangesAsync();
        }

        if (!await db.Products.AnyAsync())
        {
            var supplier = new Supplier
            {
                Name = "Mahlangu Wholesale Supplies",
                ContactName = "Operations Desk",
                Email = "orders@example.co.za",
                Phone = "011 555 0180",
                Address = "Johannesburg, Gauteng",
                IsActive = true
            };

            db.Suppliers.Add(supplier);

            var rows = new[]
            {
                ("SKU00001", "A4 Copy Paper", "Stationery", 82m, 119m, 140, 30, "600123450001"),
                ("SKU00002", "Black Ballpoint Pens", "Stationery", 2.8m, 5.5m, 380, 60, "600123450002"),
                ("SKU00003", "USB-C Cable 1m", "Electronics", 38m, 69.9m, 22, 25, "600123450003"),
                ("SKU00004", "Wireless Mouse", "Electronics", 105m, 179m, 48, 12, "600123450004")
            };

            foreach (var row in rows)
            {
                db.Products.Add(new Product
                {
                    Sku = row.Item1,
                    Name = row.Item2,
                    Category = row.Item3,
                    CostPrice = row.Item4,
                    SellingPrice = row.Item5,
                    QuantityOnHand = row.Item6,
                    ReorderLevel = row.Item7,
                    Barcode = row.Item8
                });
            }

            await db.SaveChangesAsync();

            foreach (var product in await db.Products.ToListAsync())
            {
                db.StockMovements.Add(new StockMovement
                {
                    ProductId = product.Id,
                    Type = MovementType.Opening,
                    QuantityChange = product.QuantityOnHand,
                    BalanceAfter = product.QuantityOnHand,
                    Reason = "Initial demonstration stock",
                    Reference = "OPENING",
                    PerformedBy = email
                });
            }

            db.AuditLogs.Add(new AuditLog
            {
                UserEmail = email,
                Action = "Seeded",
                EntityName = "Inventory",
                EntityId = "0",
                Details = "Initial demonstration inventory created."
            });

            await db.SaveChangesAsync();
        }
    }
}
