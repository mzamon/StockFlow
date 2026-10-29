using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Models;
namespace StockFlow.Controllers;
public class AccountController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn) : Controller
{
    [HttpGet] public IActionResult Login(string? returnUrl = null) { ViewBag.ReturnUrl = returnUrl; return View(new LoginVm()); }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Login(LoginVm vm, string? returnUrl = null) { if (!ModelState.IsValid) return View(vm); var user = await users.FindByEmailAsync(vm.Email); if (user == null || !user.IsActive) { ModelState.AddModelError("", "Invalid login or inactive account."); return View(vm); } var result = await signIn.PasswordSignInAsync(user, vm.Password, vm.RememberMe, true); if (result.Succeeded) return Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl!) : RedirectToAction("Index", "Home"); ModelState.AddModelError("", result.IsLockedOut ? "Account temporarily locked. Try again later." : "Invalid login details."); return View(vm); }
    [HttpGet] public IActionResult Register() => View(new RegisterVm());
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Register(RegisterVm vm) { if (!ModelState.IsValid) return View(vm); var u = new ApplicationUser { UserName = vm.Email, Email = vm.Email, FullName = vm.FullName, EmailConfirmed = false, IsActive = true }; var r = await users.CreateAsync(u, vm.Password); if (r.Succeeded) { await users.AddToRoleAsync(u, "Viewer"); TempData["Success"] = "Account created. An administrator can assign an operational role. No demonstration data was added to your account."; return RedirectToAction(nameof(Login)); } foreach (var e in r.Errors) ModelState.AddModelError("", e.Description); return View(vm); }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Logout() { await signIn.SignOutAsync(); return RedirectToAction("Index", "Home"); }
    public IActionResult Denied() => View();
}
