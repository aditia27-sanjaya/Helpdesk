using Hepldesk.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hepldesk.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.NamaLengkap)
                .ToListAsync();

            var userList = new List<UserListViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                userList.Add(new UserListViewModel
                {
                    User = user,
                    Roles = roles
                });
            }

            return View(userList);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                        ? "Data yang dimasukkan tidak valid."
                        : e.ErrorMessage)
                    .ToList();

                return ShowCreateUserErrors(errors);
            }

            var allowedRoles = new[] { "Admin", "Petugas" };

            if (!allowedRoles.Contains(model.Role))
            {
                return ShowCreateUserErrors(new[] { "Role yang dipilih tidak valid." });
            }

            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole<int>(model.Role));

                if (!roleResult.Succeeded)
                {
                    return ShowCreateUserErrors(roleResult.Errors.Select(e => e.Description));
                }
            }

            var user = new ApplicationUser
            {
                NamaLengkap = model.NamaLengkap.Trim(),
                UserName = model.Username.Trim(),
                Email = model.Email.Trim(),
                IsActive = true
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);

            if (!createResult.Succeeded)
            {
                return ShowCreateUserErrors(createResult.Errors.Select(e => e.Description));
            }

            var roleAssignment = await _userManager.AddToRoleAsync(user, model.Role);

            if (!roleAssignment.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                return ShowCreateUserErrors(roleAssignment.Errors.Select(e => e.Description));
            }

            TempData["SuccessMessage"] =
                $"Pengguna {user.NamaLengkap} berhasil ditambahkan.";

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user == null)
            {
                return NotFound(new { message = "Pengguna tidak ditemukan." });
            }

            var roles = await _userManager.GetRolesAsync(user);

            return Json(new
            {
                id = user.Id,
                namaLengkap = user.NamaLengkap,
                username = user.UserName,
                email = user.Email,
                role = roles.FirstOrDefault() ?? "",
                isActive = user.IsActive
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                        ? "Data yang dimasukkan tidak valid."
                        : e.ErrorMessage)
                    .ToList();

                TempData["EditUserErrors"] = System.Text.Json.JsonSerializer.Serialize(errors);

                TempData["OpenEditUserModal"] = "true";
                TempData["EditUserId"] = model.Id;

                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(model.Id.ToString());

            if (user == null)
            {
                return NotFound();
            }

            var allowedRoles = new[] { "Admin", "Petugas" };

            if (!allowedRoles.Contains(model.Role))
            {
                TempData["EditUserErrors"] = System.Text.Json.JsonSerializer.Serialize(new[] { "Role yang dipilih tidak valid." });

                TempData["OpenEditUserModal"] = "true";
                TempData["EditUserId"] = model.Id;

                return RedirectToAction(nameof(Index));
            }

            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                var roleResult = await _roleManager.CreateAsync(
                    new IdentityRole<int>(model.Role));

                if (!roleResult.Succeeded)
                {
                    TempData["EditUserErrors"] = System.Text.Json.JsonSerializer.Serialize(roleResult.Errors.Select(e => e.Description));

                    TempData["OpenEditUserModal"] = "true";
                    TempData["EditUserId"] = model.Id;

                    return RedirectToAction(nameof(Index));
                }
            }

            user.NamaLengkap = model.NamaLengkap.Trim();
            user.UserName = model.Username.Trim();
            user.Email = model.Email.Trim();
            user.IsActive = model.IsActive;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                TempData["EditUserErrors"] = System.Text.Json.JsonSerializer.Serialize(updateResult.Errors.Select(e => e.Description));

                TempData["OpenEditUserModal"] = "true";
                TempData["EditUserId"] = model.Id;

                return RedirectToAction(nameof(Index));
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!removeResult.Succeeded)
            {
                TempData["EditUserErrors"] = System.Text.Json.JsonSerializer.Serialize(removeResult.Errors.Select(e => e.Description));

                TempData["OpenEditUserModal"] = "true";
                TempData["EditUserId"] = model.Id;

                return RedirectToAction(nameof(Index));
            }

            var addRoleResult = await _userManager.AddToRoleAsync(user, model.Role);

            if (!addRoleResult.Succeeded)
            {
                TempData["EditUserErrors"] = System.Text.Json.JsonSerializer.Serialize(addRoleResult.Errors.Select(e => e.Description));

                TempData["OpenEditUserModal"] = "true";
                TempData["EditUserId"] = model.Id;

                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = $"Data pengguna {user.NamaLengkap} berhasil diperbarui.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
            ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                        ? "Data yang dimasukkan tidak valid."
                        : e.ErrorMessage)
                    .ToList();

                TempData["ResetPasswordErrors"] = System.Text.Json.JsonSerializer.Serialize(errors);

                TempData["OpenResetPasswordModal"] = "true";
                TempData["ResetPasswordUserId"] = model.Id;

                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(model.Id.ToString());

            if (user == null)
            {
                TempData["ErrorMessage"] = "Pengguna yang akan direset password tidak ditemukan.";

                return RedirectToAction(nameof(Index));
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var resetResult = await _userManager.ResetPasswordAsync(
                user,
                token,
                model.Password);

            if (!resetResult.Succeeded)
            {
                TempData["ResetPasswordErrors"] =
                    System.Text.Json.JsonSerializer.Serialize(
                        resetResult.Errors.Select(e => e.Description));

                TempData["OpenResetPasswordModal"] = "true";
                TempData["ResetPasswordUserId"] = model.Id;

                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = $"Password pengguna {user.NamaLengkap} berhasil direset.";

            return RedirectToAction(nameof(Index));
        }

        private IActionResult ShowCreateUserErrors(IEnumerable<string> errors)
        {
            TempData["OpenCreateUserModal"] = "true";

            TempData["CreateUserErrors"] =
                System.Text.Json.JsonSerializer.Serialize(errors);

            return RedirectToAction(nameof(Index));
        }
    }
}