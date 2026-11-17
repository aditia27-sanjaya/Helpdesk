using Microsoft.AspNetCore.Authorization;
using Hepldesk.Models;
using Hepldesk.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;

namespace Hepldesk.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Statistik seluruh tiket
            ViewData["TotalTiket"]    = await _context.Tikets.CountAsync();
            ViewData["TiketBaru"]     = await _context.Tikets.CountAsync(t => t.Status == "Baru");
            ViewData["TiketDiproses"] = await _context.Tikets.CountAsync(t => t.Status == "Diproses");
            ViewData["TiketSelesai"]  = await _context.Tikets.CountAsync(t => t.Status == "Selesai");

            // Ambil seluruh Petugas
            var daftarPetugas = await _userManager.GetUsersInRoleAsync("Petugas");

            // Hanya Petugas yang aktif
            var petugasAktif = daftarPetugas
                .Where(p => p.IsActive)
                .OrderBy(p => p.NamaLengkap)
                .ToList();

            // Hitung tiket aktif berdasarkan Petugas
            var jumlahTiketPetugas = await _context.Tikets
                .Where(t =>
                    t.AssignedToUserId.HasValue &&
                    (t.Status == "Baru" || t.Status == "Diproses"))
                .GroupBy(t => t.AssignedToUserId)
                .Select(g => new
                {
                    PetugasId = g.Key,
                    JumlahTiket = g.Count()
                })
                .ToListAsync();

            // Ubah hasil query menjadi Dictionary
            var dataJumlahTiket = jumlahTiketPetugas
                .ToDictionary(
                    x => x.PetugasId!.Value,
                    x => x.JumlahTiket
                );

            // Siapkan data untuk grafik batang
            ViewBag.DataTiketPetugas = petugasAktif
                .Select(p => new
                {
                    NamaPetugas = string.IsNullOrWhiteSpace(p.NamaLengkap)
                        ? p.UserName
                        : p.NamaLengkap,

                    JumlahTiket = dataJumlahTiket.ContainsKey(p.Id)
                        ? dataJumlahTiket[p.Id]
                        : 0
                })
                .ToList();

            return View("Dashboard");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
