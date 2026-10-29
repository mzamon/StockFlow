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
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        ViewBag.TotalProducts = await db.Products.CountAsync(product => product.IsActive);
        ViewBag.StockValue = await db.Products
            .Where(product => product.IsActive)
            .SumAsync(product => product.CostPrice * product.QuantityOnHand);
        ViewBag.LowStock = await db.Products.CountAsync(
            product => product.IsActive && product.QuantityOnHand <= product.ReorderLevel);
        ViewBag.Sales = await db.Sales
            .Where(sale => sale.SoldAt >= thirtyDaysAgo)
            .SumAsync(sale => (decimal?)sale.Total) ?? 0;
        ViewBag.Trends = await db.Sales
            .Where(sale => sale.SoldAt >= thirtyDaysAgo)
            .GroupBy(sale => sale.SoldAt.Date)
            .Select(group => new
            {
                Date = group.Key,
                Total = group.Sum(sale => sale.Total)
            })
            .OrderBy(trend => trend.Date)
            .ToListAsync();

        return View(await db.Products
            .Where(product => product.IsActive)
            .OrderBy(product => product.QuantityOnHand - product.ReorderLevel)
            .ToListAsync());
    }

    public async Task<IActionResult> ExportProducts()
    {
        var products = await db.Products
            .OrderBy(product => product.Name)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine(
            "SKU,Name,Category,CostPrice,SellingPrice,QuantityOnHand,ReorderLevel,Barcode,Status");

        foreach (var product in products)
        {
            var values = new[]
            {
                product.Sku,
                product.Name,
                product.Category,
                product.CostPrice.ToString("0.00"),
                product.SellingPrice.ToString("0.00"),
                product.QuantityOnHand.ToString(),
                product.ReorderLevel.ToString(),
                product.Barcode ?? string.Empty,
                product.IsActive ? "Active" : "Inactive"
            };

            sb.AppendLine(string.Join(',', values.Select(Csv)));
        }

        return File(
            Encoding.UTF8.GetBytes(sb.ToString()),
            "text/csv",
            "stockflow-products.csv");
    }

    private static string Csv(string? value)
    {
        value ??= string.Empty;
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
