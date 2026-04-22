using AutoMapper;
using BookExchange.Db.Data;
using BookExchange.Db.Entities;
using BookExchange.Db.Interfaces;
using BookExchange.Web.Data;
using BookExchange.Web.Helpers;
using BookExchange.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookExchange.Web.Controllers;

/// <summary>
/// Регистрация / вход / выход + личный профиль с вкладками.
/// НЕ используем scaffold-страницы Identity (папки Pages нет), всё через свои View.
/// </summary>
public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _env;

    public AccountController(UserManager<User> um, SignInManager<User> sm, IUnitOfWork uow, IMapper mapper, IWebHostEnvironment env)
    {
        _userManager = um;
        _signInManager = sm;
        _uow = uow;
        _mapper = mapper;
        _env = env;
    }

    // ======================== Register ========================
    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new User
        {
            UserName = model.UserName,
            Email = model.Email,
            RegistrationDate = DateTime.UtcNow
        };
        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, DbSeeder.UserRole);
        await _signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToAction("Index", "Home");
    }

    // ======================== Login ========================
    [HttpGet]
    public IActionResult Login(string? returnUrl = null) =>
        View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Неверный email или пароль.");
            return View(model);
        }

        if (user.IsBlocked)
        {
            ModelState.AddModelError(string.Empty, "Ваша учётная запись заблокирована.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Неверный email или пароль.");
            return View(model);
        }

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    // ======================== Logout ========================
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();

    // ======================== Profile (свой) ========================
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Login));
        return RedirectToAction("Details", "User", new { id = user.Id });
    }

    // ======================== Edit Profile ========================
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> EditProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Login));

        return View(new EditProfileViewModel
        {
            UserName = user.UserName ?? string.Empty,
            Location = user.Location,
            ExistingAvatarPath = user.AvatarPath
        });
    }

    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EditProfileViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction(nameof(Login));

        if (!ModelState.IsValid)
        {
            model.ExistingAvatarPath = user.AvatarPath;
            return View(model);
        }

        user.UserName = model.UserName;
        user.Location = model.Location;

        if (model.Avatar != null)
        {
            var path = await ImageHelper.SaveAsync(model.Avatar, _env, "images/avatars");
            if (path != null) user.AvatarPath = path;
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var e in updateResult.Errors) ModelState.AddModelError(string.Empty, e.Description);
            model.ExistingAvatarPath = user.AvatarPath;
            return View(model);
        }

        // Смена пароля (если оба поля заполнены).
        if (!string.IsNullOrWhiteSpace(model.NewPassword))
        {
            if (string.IsNullOrWhiteSpace(model.CurrentPassword))
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "Укажите текущий пароль.");
                model.ExistingAvatarPath = user.AvatarPath;
                return View(model);
            }
            var pw = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!pw.Succeeded)
            {
                foreach (var e in pw.Errors) ModelState.AddModelError(string.Empty, e.Description);
                model.ExistingAvatarPath = user.AvatarPath;
                return View(model);
            }
        }

        TempData["Success"] = "Профиль обновлён.";
        return RedirectToAction(nameof(Profile));
    }
}
