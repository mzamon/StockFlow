using System.ComponentModel.DataAnnotations;
using StockFlow.Models;
namespace StockFlow.Models;
public class LoginVm { [Required, EmailAddress] public string Email { get; set; } = ""; [Required, DataType(DataType.Password)] public string Password { get; set; } = ""; public bool RememberMe { get; set; } }
public class RegisterVm { [Required, MaxLength(120)] public string FullName { get; set; } = ""; [Required, EmailAddress] public string Email { get; set; } = ""; [Required, MinLength(10), DataType(DataType.Password)] public string Password { get; set; } = ""; [Required, Compare(nameof(Password)), DataType(DataType.Password)] public string ConfirmPassword { get; set; } = ""; }
public class DashboardVm { public int Products { get; set; } public int LowStock { get; set; } public int PendingOrders { get; set; } public int SalesToday { get; set; } public decimal StockValue { get; set; } public List<Product> LowStockItems { get; set; } = new(); public List<StockMovement> RecentMovements { get; set; } = new(); public List<Sale> RecentSales { get; set; } = new(); }
public class AdjustmentVm { public int ProductId { get; set; } [Range(-100000, 100000)] public int QuantityChange { get; set; } [Required, MaxLength(300)] public string Reason { get; set; } = ""; }
public class CreatePurchaseVm { public int SupplierId { get; set; } public DateTime? ExpectedAt { get; set; } public string? Notes { get; set; } public int ProductId { get; set; } [Range(1, 100000)] public int Quantity { get; set; } [Range(0, 999999)] public decimal UnitCost { get; set; } }
public class CreateSaleVm { public string? CustomerName { get; set; } public int ProductId { get; set; } [Range(1, 100000)] public int Quantity { get; set; } }
