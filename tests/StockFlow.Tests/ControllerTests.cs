using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockFlow.Controllers;
using StockFlow.Data;
using StockFlow.Models;
using Xunit;

namespace StockFlow.Tests;

public class ControllerTests
{
    [Fact]
    public async Task Dashboard_ReturnsEmptyViewModelForNonAdmin()
    {
        await using var db = CreateDatabase();
        db.Products.Add(new Product
        {
            Sku = "SKU-1",
            Name = "Private product",
            CostPrice = 10,
            SellingPrice = 15,
            QuantityOnHand = 4
        });
        await db.SaveChangesAsync();

        var controller = new HomeController(db);
        SetUser(controller, "viewer@example.com", "Viewer");

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<DashboardVm>(view.Model);
        Assert.Equal(0, model.Products);
        Assert.Empty(model.LowStockItems);
        Assert.Empty(model.RecentMovements);
        Assert.Empty(model.RecentSales);
    }

    [Fact]
    public async Task Dashboard_LimitsAdminListsToFourRecords()
    {
        await using var db = CreateDatabase();

        for (var index = 1; index <= 6; index++)
        {
            db.Products.Add(new Product
            {
                Sku = $"SKU-{index}",
                Name = $"Product {index}",
                CostPrice = 10,
                SellingPrice = 15,
                QuantityOnHand = 0,
                ReorderLevel = 5
            });
        }

        for (var index = 1; index <= 6; index++)
        {
            db.StockMovements.Add(new StockMovement
            {
                ProductId = index,
                QuantityChange = 1,
                BalanceAfter = 1,
                Reference = $"MOV-{index}",
                PerformedBy = "admin@example.com"
            });
            db.Sales.Add(new Sale
            {
                SaleNumber = $"SALE-{index}",
                Total = index
            });
        }

        await db.SaveChangesAsync();

        var controller = new HomeController(db);
        SetUser(controller, "admin@example.com", "Admin");

        var result = await controller.Index();

        var model = Assert.IsType<DashboardVm>(Assert.IsType<ViewResult>(result).Model);
        Assert.Equal(6, model.Products);
        Assert.Equal(4, model.LowStockItems.Count);
        Assert.Equal(4, model.RecentMovements.Count);
        Assert.Equal(4, model.RecentSales.Count);
    }

    [Fact]
    public async Task ProductExport_EscapesEmbeddedQuotes()
    {
        await using var db = CreateDatabase();
        db.Products.Add(new Product
        {
            Sku = "SKU-1",
            Name = "Cable \"Premium\"",
            Category = "Electronics",
            CostPrice = 10,
            SellingPrice = 15,
            QuantityOnHand = 2,
            ReorderLevel = 1
        });
        await db.SaveChangesAsync();

        var controller = new ReportsController(db);
        SetUser(controller, "admin@example.com", "Admin");

        var result = await controller.ExportProducts();

        var file = Assert.IsType<FileContentResult>(result);
        var csv = System.Text.Encoding.UTF8.GetString(file.FileContents);
        Assert.Contains("\"Cable \"\"Premium\"\"\"", csv);
        Assert.Contains("SKU,Name,Category,CostPrice,SellingPrice,QuantityOnHand,ReorderLevel,Barcode,Status", csv);
    }

    [Fact]
    public async Task ProductAdjustment_RejectsNegativeStock()
    {
        await using var db = CreateDatabase();
        var product = new Product
        {
            Sku = "SKU-1",
            Name = "Test product",
            QuantityOnHand = 2
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var controller = new ProductsController(db);
        SetUser(controller, "clerk@example.com", "StockClerk");
        var model = new AdjustmentVm
        {
            ProductId = product.Id,
            QuantityChange = -3,
            Reason = "Invalid correction"
        };

        var result = await controller.Adjust(model);

        Assert.IsType<ViewResult>(result);
        Assert.Contains(controller.ModelState.Values.SelectMany(value => value.Errors),
            error => error.ErrorMessage!.Contains("negative stock"));
        Assert.Equal(2, (await db.Products.FindAsync(product.Id))!.QuantityOnHand);
    }

    private static ApplicationDbContext CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static void SetUser(Controller controller, string email, string role)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[]
                    {
                        new Claim(ClaimTypes.Name, email),
                        new Claim(ClaimTypes.Role, role)
                    },
                    "TestAuthentication"))
            }
        };
    }
}
