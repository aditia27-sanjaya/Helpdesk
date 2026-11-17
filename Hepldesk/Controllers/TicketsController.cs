using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.IO;
using SixLabors.ImageSharp;
using Hepldesk.Data;
using Hepldesk.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Hepldesk.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TicketsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Ambil daftar akun dengan role Petugas
            var daftarPetugas =
                await _userManager.GetUsersInRoleAsync("Petugas");

            // Filter hanya akun yang aktif
            var petugasAktif = daftarPetugas
                .Where(p => p.IsActive)
                .OrderBy(p => p.UserName)
                .ToList();

            // Kirim daftar Petugas ke View
            ViewBag.Petugas = petugasAktif;

            // Kirim nama Petugas berdasarkan ID
            ViewBag.NamaPetugas = daftarPetugas
                .ToDictionary(p => p.Id, p => p.UserName);

            // Query tiket
            var queryTiket = _context.Tikets.AsQueryable();

            if (User.IsInRole("Admin"))
            {
                // Admin dapat melihat seluruh tiket
            }
            else if (User.IsInRole("Petugas"))
            {
                // Ambil akun Petugas yang sedang login
                var pengguna = await _userManager.GetUserAsync(User);

                if (pengguna == null)
                {
                    return Forbid();
                }

                // Petugas hanya melihat tiket yang ditugaskan kepadanya
                queryTiket = queryTiket.Where(t =>
                    t.AssignedToUserId == pengguna.Id);
            }
            else
            {
                return Forbid();
            }

            // Ambil tiket berdasarkan hak akses
            var tikets = await queryTiket
                .OrderByDescending(t => t.DateCreate)
                .ToListAsync();

            return View(tikets);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tiket tiket, IFormFile? Screenshot, int? AssignedToUserId)
        {
            // Status awal selalu Baru
            tiket.Status     = "Baru";
            tiket.DateCreate = DateTime.Now;
            tiket.DateUpdate = null;

            // Validasi Petugas yang dipilih
            if (AssignedToUserId.HasValue)
            {
                var daftarPetugas = await _userManager.GetUsersInRoleAsync("Petugas");
                var petugasValid = daftarPetugas.Any(p =>
                    p.Id == AssignedToUserId.Value &&
                    p.IsActive);

                if (!petugasValid)
                {
                    TempData["ErrorMessage"] =
                        "Petugas yang dipilih tidak valid atau sudah tidak aktif.";

                    return RedirectToAction(nameof(Index));
                }
            }

            // Simpan ID Petugas yang dipilih
            tiket.AssignedToUserId = AssignedToUserId;

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Judul atau keterangan tiket tidak valid.";
                return RedirectToAction(nameof(Index));
            }

            string? pathFile = null;

            if (Screenshot != null && Screenshot.Length > 0)
            {
                if (Screenshot.Length > 2 * 1024 * 1024)
                {
                    TempData["ErrorMessage"] = "Ukuran screenshot maksimal 2 MB.";
                    return RedirectToAction(nameof(Index));
                }

                var ekstensi = Path.GetExtension(Screenshot.FileName).ToLowerInvariant();

                if (ekstensi != ".jpg" &&
                    ekstensi != ".jpeg" &&
                    ekstensi != ".png")
                {
                    TempData["ErrorMessage"] = "Format screenshot harus JPG atau PNG.";
                    return RedirectToAction(nameof(Index));
                }

                try
                {
                    using var gambar = await Image.LoadAsync(Screenshot.OpenReadStream());

                    var format = gambar.Metadata.DecodedImageFormat?.Name;

                    if (format != "JPEG" && format != "PNG")
                    {
                        TempData["ErrorMessage"] = "Isi file harus berupa gambar JPG atau PNG.";

                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (UnknownImageFormatException)
                {
                    TempData["ErrorMessage"] = "File yang diunggah bukan gambar yang valid.";

                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidImageContentException)
                {
                    TempData["ErrorMessage"] = "Isi file gambar rusak atau tidak valid.";

                    return RedirectToAction(nameof(Index));
                }

                var namaFile = $"{Guid.NewGuid()}{ekstensi}";

                var folderUpload = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "uploads",
                    "tiket");

                Directory.CreateDirectory(folderUpload);

                var lokasiFile = Path.Combine(folderUpload, namaFile);

                using (var stream = new FileStream(lokasiFile, FileMode.Create))
                {
                    await Screenshot.CopyToAsync(stream);
                }

                pathFile = $"/uploads/tiket/{namaFile}";
                tiket.ScreenshotPath = pathFile;
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Simpan tiket
                _context.Tikets.Add(tiket);

                // Simpan riwayat status awal
                var riwayat = new TiketStatusHistory
                {
                    Tiket            = tiket,
                    Status           = tiket.Status,
                    TanggalPerubahan = tiket.DateCreate,
                    Keterangan       = "Tiket berhasil dibuat oleh Admin."
                };

                _context.TiketStatusHistories.Add(riwayat);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();

                if (pathFile != null)
                {
                    var folderUpload = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "uploads",
                        "tiket");

                    var lokasiFile = Path.Combine(
                        folderUpload,
                        Path.GetFileName(pathFile));

                    if (System.IO.File.Exists(lokasiFile))
                    {
                        System.IO.File.Delete(lokasiFile);
                    }
                }

                throw;
            }

            TempData["SuccessMessage"] = "Tiket berhasil ditambahkan!";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Petugas")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Tiket tiket, IFormFile? Screenshot, string? KeteranganStatus, int? AssignedToUserId)
        {
            var tiketLama = await _context.Tikets.FindAsync(tiket.Id);

            if (tiketLama == null)
            {
                return NotFound();
            }

            // Hanya Admin yang boleh mengubah penugasan
            if (User.IsInRole("Admin"))
            {
                if (AssignedToUserId.HasValue)
                {
                    var daftarPetugas = await _userManager.GetUsersInRoleAsync("Petugas");
                    var petugasValid = daftarPetugas.Any(p =>
                        p.Id == AssignedToUserId.Value &&
                        p.IsActive);

                    if (!petugasValid)
                    {
                        TempData["ErrorMessage"] = "Petugas yang dipilih tidak valid atau sudah tidak aktif.";

                        return RedirectToAction(nameof(Index));
                    }
                }
                tiketLama.AssignedToUserId = AssignedToUserId;
            }
            else
            {
                // Petugas hanya boleh menangani tiket miliknya sendiri
                var pengguna = await _userManager.GetUserAsync(User);
                if (pengguna == null ||
                    tiketLama.AssignedToUserId != pengguna.Id)
                {
                    return Forbid();
                }
            }

            // Periksa apakah status berubah
            bool statusBerubah = tiketLama.Status != tiket.Status;

            // Keterangan wajib jika status berubah
            if (statusBerubah &&
                string.IsNullOrWhiteSpace(KeteranganStatus))
            {
                TempData["ErrorMessage"] =
                    "Keterangan wajib diisi jika status tiket berubah.";

                return RedirectToAction(nameof(Index));
            }

            // Validasi panjang keterangan
            if (!string.IsNullOrWhiteSpace(KeteranganStatus) &&
                KeteranganStatus.Length > 500)
            {
                TempData["ErrorMessage"] =
                    "Keterangan perubahan status maksimal 500 karakter.";

                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] =
                    "Judul, keterangan, atau status tiket tidak valid.";

                return RedirectToAction(nameof(Index));
            }

            string? screenshotLamaPath = tiketLama.ScreenshotPath;
            string? screenshotBaruPath = null;
            string? pathFileBaru = null;

            var folderUpload = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "tiket"
            );

            if (Screenshot != null && Screenshot.Length > 0)
            {
                if (Screenshot.Length > 2 * 1024 * 1024)
                {
                    TempData["ErrorMessage"] =
                        "Ukuran screenshot maksimal 2 MB.";

                    return RedirectToAction(nameof(Index));
                }

                var ekstensi = Path.GetExtension(
                    Screenshot.FileName).ToLowerInvariant();

                if (ekstensi != ".jpg" &&
                    ekstensi != ".jpeg" &&
                    ekstensi != ".png")
                {
                    TempData["ErrorMessage"] =
                        "Format screenshot harus JPG atau PNG.";

                    return RedirectToAction(nameof(Index));
                }

                try
                {
                    using var gambar = await Image.LoadAsync(
                        Screenshot.OpenReadStream());

                    var format = gambar.Metadata.DecodedImageFormat?.Name;

                    if (format != "JPEG" && format != "PNG")
                    {
                        TempData["ErrorMessage"] =
                            "Isi file harus berupa gambar JPG atau PNG.";

                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (UnknownImageFormatException)
                {
                    TempData["ErrorMessage"] =
                        "File yang diunggah bukan gambar yang valid.";

                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidImageContentException)
                {
                    TempData["ErrorMessage"] =
                        "Isi file gambar rusak atau tidak valid.";

                    return RedirectToAction(nameof(Index));
                }

                var namaFile = $"{Guid.NewGuid()}{ekstensi}";

                Directory.CreateDirectory(folderUpload);

                pathFileBaru = Path.Combine(folderUpload, namaFile);

                try
                {
                    await using (var stream = new FileStream(
                        pathFileBaru,
                        FileMode.CreateNew))
                    {
                        await Screenshot.CopyToAsync(stream);
                    }
                }
                catch (IOException)
                {
                    TempData["ErrorMessage"] =
                        "Screenshot gagal disimpan. Silakan coba kembali.";

                    return RedirectToAction(nameof(Index));
                }

                screenshotBaruPath = $"/uploads/tiket/{namaFile}";
            }

            var tanggalPerubahan = DateTime.Now;

            // Perbarui data tiket
            tiketLama.Judul = tiket.Judul;
            tiketLama.Status = tiket.Status;
            tiketLama.Keterangan = tiket.Keterangan;
            tiketLama.DateUpdate = tanggalPerubahan;

            if (screenshotBaruPath != null)
            {
                tiketLama.ScreenshotPath = screenshotBaruPath;
            }

            // Simpan riwayat hanya jika status berubah
            if (statusBerubah)
            {
                var riwayat = new TiketStatusHistory
                {
                    TiketId = tiketLama.Id,
                    Status = tiket.Status,
                    TanggalPerubahan = tanggalPerubahan,
                    Keterangan = KeteranganStatus!.Trim()
                };

                _context.TiketStatusHistories.Add(riwayat);
            }

            // Simpan tiket dan riwayat dalam satu transaksi
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();

                if (pathFileBaru != null &&
                    System.IO.File.Exists(pathFileBaru))
                {
                    System.IO.File.Delete(pathFileBaru);
                }

                throw;
            }

            // Hapus screenshot lama setelah penyimpanan berhasil
            if (screenshotBaruPath != null &&
                !string.IsNullOrWhiteSpace(screenshotLamaPath) &&
                screenshotLamaPath.StartsWith(
                    "/uploads/tiket/",
                    StringComparison.OrdinalIgnoreCase))
            {
                var namaFileLama = Path.GetFileName(screenshotLamaPath);

                var pathFileLama = Path.Combine(
                    folderUpload,
                    namaFileLama);

                if (System.IO.File.Exists(pathFileLama))
                {
                    System.IO.File.Delete(pathFileLama);
                }
            }

            TempData["SuccessMessage"] =
                statusBerubah
                    ? "Tiket dan riwayat status berhasil diperbarui!"
                    : "Tiket berhasil diperbarui!";

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id) 
        {
            var tiket = await _context.Tikets.FindAsync(id);

            if (tiket == null)
            {  
                return NotFound(); 
            }

            _context.Tikets.Remove(tiket); await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Tiket berhasil dihapus!";

            return RedirectToAction(nameof(Index));
        }

    }
}
