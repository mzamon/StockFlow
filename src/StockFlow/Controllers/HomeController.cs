using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockFlow.Data;
using StockFlow.Models;
namespace StockFlow.Controllers;
public class HomeController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index() { var vm = new DashboardVm { Products = await db.Products.CountAsync(x => x.IsActive), LowStock = await db.Products.CountAsync(x => x.IsActive && x.QuantityOnHand <= x.ReorderLevel), PendingOrders = await db.PurchaseOrders.CountAsync(x => x.Status == PurchaseStatus.PendingApproval), SalesToday = await db.Sales.CountAsync(x => x.SoldAt.Date == DateTime.UtcNow.Date), StockValue = await db.Products.Where(x => x.IsActive).SumAsync(x => x.CostPrice * x.QuantityOnHand), LowStockItems = await db.Products.Where(x => x.IsActive && x.QuantityOnHand <= x.ReorderLevel).OrderBy(x => x.QuantityOnHand).Take(6).ToListAsync(), RecentMovements = await db.StockMovements.Include(x => x.Product).OrderByDescending(x => x.CreatedAt).Take(6).ToListAsync(), RecentSales = await db.Sales.OrderByDescending(x => x.SoldAt).Take(5).ToListAsync() }; return View(vm); }
    public IActionResult Error() => View();
}
