using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StockFlow.Data;
using StockFlow.Models;
using StockFlow.Services;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(o => { o.Password.RequiredLength = 10; o.Password.RequireDigit = true; o.Password.RequireUppercase = true; o.Password.RequireLowercase = true; o.Password.RequireNonAlphanumeric = true; o.User.RequireUniqueEmail = true; o.Lockout.MaxFailedAccessAttempts = 5; o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); })
 .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(o => { o.LoginPath = "/Account/Login"; o.AccessDeniedPath = "/Account/Denied"; o.SlidingExpiration = true; });
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuditService>();
var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
app.UseHttpsRedirection(); app.UseStaticFiles(); app.UseRouting(); app.UseAuthentication(); app.UseAuthorization();
using (var scope = app.Services.CreateScope()) { var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(); await db.Database.EnsureCreatedAsync(); await SeedData.Initialize(scope.ServiceProvider); }
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
