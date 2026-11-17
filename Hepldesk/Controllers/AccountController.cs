
using Hepldesk.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hepldesk.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly IWebHostEnvironment _environment;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            IWebHostEnvironment environment)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _roleManager = roleManager;
            _environment = environment;
        }

        // Memastikan setup hanya tersedia saat Development
        // dan database belum memiliki pengguna.
        private async Task<bool> CanSetupAdminAsync()
        {
            if (!_environment.IsDevelopment())
            {
                return false;
            }

            return !await _userManager.Users.AnyAsync();
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model,
            string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByNameAsync(model.Username);

            if (user == null || !user.IsActive)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Username atau password tidak valid, atau akun tidak aktif.");

                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) &&
                    Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(
                string.Empty,
                "Username atau password tidak valid, atau akun tidak aktif.");

            return View(model);
        }

        // Form untuk membuat Admin pertama
        [HttpGet]
        public async Task<IActionResult> SetupAdmin()
        {
            if (!await CanSetupAdminAsync())
            {
                return NotFound();
            }

            return View(new RegisterViewModel());
        }

        // Memproses pembuatan Admin pertama
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetupAdmin(
            RegisterViewModel model)
        {
            if (!await CanSetupAdminAsync())
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Pastikan role Admin tersedia
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                var roleResult = await _roleManager.CreateAsync(
                    new IdentityRole<int>("Admin"));

                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                    {
                        ModelState.AddModelError(
                            string.Empty, error.Description);
                    }

                    return View(model);
                }
            }

            var user = new ApplicationUser
            {
                UserName = model.Username.Trim(),
                Email = model.Email.Trim(),
                NamaLengkap = model.NamaLengkap.Trim(),
                IsActive = true
            };

            var createResult = await _userManager.CreateAsync(
                user, model.Password);

            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty, error.Description);
                }

                return View(model);
            }

            // Tetapkan role Admin kepada akun pertama
            var roleAssignment = await _userManager.AddToRoleAsync(
                user, "Admin");

            if (!roleAssignment.Succeeded)
            {
                // Bersihkan akun jika penetapan role gagal
                await _userManager.DeleteAsync(user);

                foreach (var error in roleAssignment.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty, error.Description);
                }

                return View(model);
            }

            TempData["SuccessMessage"] =
                "Akun Admin pertama berhasil dibuat. Silakan login.";

            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}