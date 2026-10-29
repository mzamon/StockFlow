using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Models;
namespace StockFlow.Controllers;
public class AccountController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginVm());
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm vm, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var user = await users.FindByEmailAsync(vm.Email);
        if (user == null || !user.IsActive)
        {
            ModelState.AddModelError("", "Invalid login or inactive account.");
            return View(vm);
        }

        var result = await signIn.PasswordSignInAsync(user, vm.Password, vm.RememberMe, true);
        if (result.Succeeded)
        {
            return Url.IsLocalUrl(returnUrl)
                ? Redirect(returnUrl!)
                : RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(
            "",
            result.IsLockedOut
                ? "Account temporarily locked. Try again later."
                : "Invalid login details.");
        return View(vm);
    }
    [HttpGet]
    public IActionResult Register()
    {
        return User.Identity?.IsAuthenticated == true
            ? RedirectToAction("Index", "Home")
            : View(new RegisterVm());
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var user = new ApplicationUser
        {
            UserName = vm.Email,
            Email = vm.Email,
            FullName = vm.FullName,
            EmailConfirmed = false,
            IsActive = true
        };

        var result = await users.CreateAsync(user, vm.Password);
        if (result.Succeeded)
        {
            await users.AddToRoleAsync(user, "Viewer");
            TempData["Success"] = "Account created. Sign in to continue.";
            return RedirectToAction(nameof(Login));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return View(vm);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signIn.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }
    public IActionResult Denied() => View();
}
