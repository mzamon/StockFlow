using StockFlow.Data;
using StockFlow.Models;
namespace StockFlow.Services;
public class AuditService(ApplicationDbContext db, IHttpContextAccessor? accessor = null) { public async Task WriteAsync(string action, string entity, string id, string details = "") { db.AuditLogs.Add(new AuditLog { UserEmail = "system", Action = action, EntityName = entity, EntityId = id, Details = details }); await db.SaveChangesAsync(); } }
