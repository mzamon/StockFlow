using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockFlow.Data;
using StockFlow.Models;

namespace StockFlow.Controllers;

[Authorize]
public class HomeController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        if (!User.IsInRole("Admin"))
        {
            return View(new DashboardVm());
        }

        var today = DateTime.UtcNow.Date;
        var vm = new DashboardVm
        {
            Products = await db.Products.CountAsync(x => x.IsActive),
            LowStock = await db.Products.CountAsync(
                x => x.IsActive && x.QuantityOnHand <= x.ReorderLevel),
            PendingOrders = await db.PurchaseOrders.CountAsync(
                x => x.Status == PurchaseStatus.PendingApproval),
            SalesToday = await db.Sales.CountAsync(x => x.SoldAt >= today),
            StockValue = await db.Products
                .Where(x => x.IsActive)
                .SumAsync(x => x.CostPrice * x.QuantityOnHand),
            LowStockItems = await db.Products
                .Where(x => x.IsActive && x.QuantityOnHand <= x.ReorderLevel)
                .OrderBy(x => x.QuantityOnHand)
                .Take(4)
                .ToListAsync(),
            RecentMovements = await db.StockMovements
                .Include(x => x.Product)
                .OrderByDescending(x => x.CreatedAt)
                .Take(4)
                .ToListAsync(),
            RecentSales = await db.Sales
                .OrderByDescending(x => x.SoldAt)
                .Take(4)
                .ToListAsync()
        };

        return View(vm);
    }

    public IActionResult Error() => View();
}
