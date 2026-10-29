using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockFlow.Data;
namespace StockFlow.Controllers; [Authorize(Roles = "Admin,Manager,StockClerk,Procurement,Viewer")] public class ActivityController(ApplicationDbContext db) : Controller { public async Task<IActionResult> Index(string? q) { var x = db.StockMovements.Include(m => m.Product).AsQueryable(); if (!string.IsNullOrWhiteSpace(q)) x = x.Where(m => m.Product!.Name.Contains(q) || m.Reference.Contains(q) || m.Reason.Contains(q)); ViewBag.Query = q; return View(await x.OrderByDescending(m => m.CreatedAt).Take(500).ToListAsync()); } [Authorize(Roles = "Admin,Manager")] public async Task<IActionResult> Audit() => View(await db.AuditLogs.OrderByDescending(a => a.CreatedAt).Take(500).ToListAsync()); }
