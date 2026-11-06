using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockFlow.Data;

namespace StockFlow.Controllers;

[Authorize(Roles = "Admin,Manager,Procurement,Viewer")]
public class ReportsController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.TotalProducts = await db.Products.CountAsync(p => p.IsActive);
        ViewBag.StockValue = await db
            .Products.Where(p => p.IsActive)
            .SumAsync(p => p.CostPrice * p.QuantityOnHand);
        ViewBag.LowStock = await db.Products.CountAsync(p =>
            p.IsActive && p.QuantityOnHand <= p.ReorderLevel
        );
        ViewBag.Sales =
            await db
                .Sales.Where(s => s.SoldAt >= DateTime.UtcNow.AddDays(-30))
                .SumAsync(s => (decimal?)s.Total)
            ?? 0;
        ViewBag.Trends = await db
            .Sales.Where(s => s.SoldAt >= DateTime.UtcNow.AddDays(-30))
            .GroupBy(s => s.SoldAt.Date)
            .Select(g => new { Date = g.Key, Total = g.Sum(x => x.Total) })
            .OrderBy(x => x.Date)
            .ToListAsync();

        return View(
            await db
                .Products.Where(p => p.IsActive)
                .OrderBy(p => p.QuantityOnHand - p.ReorderLevel)
                .ToListAsync()
        );
    }

    public async Task<IActionResult> ExportProducts()
    {
        var products = await db.Products.OrderBy(p => p.Name).ToListAsync();
        var sb = new StringBuilder(
            "SKU,Name,Category,CostPrice,SellingPrice,QuantityOnHand,ReorderLevel,Barcode,Status\n"
        );

        foreach (var product in products)
        {
            sb.AppendLine(
                string.Join(
                    ',',
                    new[]
                    {
                        product.Sku,
                        product.Name,
                        product.Category,
                        product.CostPrice.ToString("0.00"),
                        product.SellingPrice.ToString("0.00"),
                        product.QuantityOnHand.ToString(),
                        product.ReorderLevel.ToString(),
                        product.Barcode ?? "",
                        product.IsActive ? "Active" : "Inactive",
                    }.Select(Csv)
                )
            );
        }

        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "stockflow-products.csv");
    }

    private static string Csv(string value) => $"\"{value.Replace("\"", "\"\"")}\"";
}
