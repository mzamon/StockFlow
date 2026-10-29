using StockFlow.Data;
using StockFlow.Models;

namespace StockFlow.Services;

public class AuditService(ApplicationDbContext db, IHttpContextAccessor accessor)
{
    public async Task WriteAsync(
        string action,
        string entity,
        string id,
        string details = "")
    {
        var userEmail = accessor.HttpContext?.User.Identity?.Name ?? "system";

        db.AuditLogs.Add(new AuditLog
        {
            UserEmail = userEmail,
            Action = action,
            EntityName = entity,
            EntityId = id,
            Details = details
        });

        await db.SaveChangesAsync();
    }
}
