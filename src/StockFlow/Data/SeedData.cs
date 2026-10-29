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
        // Demo inventory is installed only in a fresh database and is owned by the seeded admin account.
        if (!await db.Products.AnyAsync()) { var supplier = new Supplier { Name = "Mahlangu Wholesale Supplies", ContactName = "Operations Desk", Email = "orders@example.co.za", Phone = "011 555 0180", Address = "Johannesburg, Gauteng", IsActive = true }; db.Suppliers.Add(supplier); var rows = new[] { ("SKU00001", "A4 Copy Paper", "Stationery", 82m, 119m, 140, 30, "600123450001"), ("SKU00002", "Black Ballpoint Pens", "Stationery", 2.8m, 5.5m, 380, 60, "600123450002"), ("SKU00003", "USB-C Cable 1m", "Electronics", 38m, 69.9m, 22, 25, "600123450003"), ("SKU00004", "Wireless Mouse", "Electronics", 105m, 179m, 48, 12, "600123450004"), ("SKU00005", "Packing Tape", "Packaging", 14m, 25m, 9, 20, "600123450005"), ("SKU00006", "Thermal Labels", "Packaging", 45m, 79m, 72, 15, "600123450006"), ("SKU00007", "Safety Gloves", "Safety", 22m, 39.5m, 16, 20, "600123450007"), ("SKU00008", "Cleaning Solution 1L", "Cleaning", 31m, 55m, 64, 15, "600123450008") }; foreach (var x in rows) db.Products.Add(new Product { Sku = x.Item1, Name = x.Item2, Category = x.Item3, CostPrice = x.Item4, SellingPrice = x.Item5, QuantityOnHand = x.Item6, ReorderLevel = x.Item7, Barcode = x.Item8 }); await db.SaveChangesAsync(); foreach (var p in db.Products) db.StockMovements.Add(new StockMovement { ProductId = p.Id, Type = MovementType.Opening, QuantityChange = p.QuantityOnHand, BalanceAfter = p.QuantityOnHand, Reason = "Initial demonstration stock", Reference = "OPENING", PerformedBy = email }); await db.SaveChangesAsync(); db.AuditLogs.Add(new AuditLog { UserEmail = email, Action = "Seeded", EntityName = "Inventory", EntityId = "0", Details = "Initial demonstration inventory created." }); await db.SaveChangesAsync(); }
    }
}
