
using System.Security.Cryptography;
using System.Text;
using Hepldesk.Data;
using Hepldesk.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;

namespace Hepldesk.Controllers
{
    [AllowAnonymous]
    public class PublicController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public PublicController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Track()
        {
            return View(new PublicTiketTrackViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Track(PublicTiketTrackViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Cari tiket berdasarkan nomor tiket publik
            var tiket = await _context.Tikets
                .FirstOrDefaultAsync(t =>
                    t.NomorTiketPublik == model.NomorTiketPublik.Trim() &&
                    t.IzinkanPelacakanPublik);

            // Tiket tidak ditemukan atau pelacakan tidak diizinkan
            if (tiket == null ||
                string.IsNullOrWhiteSpace(tiket.KodePelacakanHash))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Nomor tiket atau kode pelacakan tidak valid.");

                return View(model);
            }

            // Hitung hash kode pelacakan yang dimasukkan pengguna
            var kodeHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(model.KodePelacakan)));

            // Bandingkan hash secara aman
            var hashTersimpan = Convert.FromHexString(tiket.KodePelacakanHash);

            var hashInput = Convert.FromHexString(kodeHash);

            if (!CryptographicOperations.FixedTimeEquals(
                hashTersimpan, hashInput))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Nomor tiket atau kode pelacakan tidak valid.");

                return View(model);
            }

            // Ambil riwayat status tiket
            var riwayat = await _context.TiketStatusHistories
                .Where(h => h.TiketId == tiket.Id)
                .OrderBy(h => h.TanggalPerubahan)
                .ThenBy(h => h.Id)
                .Select(h => new PublicTiketStatusHistoryViewModel
                {
                    Status = h.Status,
                    TanggalPerubahan = h.TanggalPerubahan,
                    Keterangan = h.Keterangan
                })
                .ToListAsync();

            // Siapkan hasil pelacakan
            var hasil = new PublicTiketTrackResultViewModel
            {
                NomorTiketPublik = tiket.NomorTiketPublik!,
                Judul = tiket.Judul,
                Status = tiket.Status,
                TanggalDibuat = tiket.DateCreate,
                RiwayatStatus = riwayat
            };

            return View("Track", hasil);
        }

        // Menampilkan formulir pengaduan
        [HttpGet]
        public IActionResult Create()
        {
            return View(new PublicTiketViewModel());
        }

        // Memproses pengaduan baru
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            PublicTiketViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Create", model);
            }

            string? pathFile = null;

            // Validasi screenshot jika diunggah
            if (model.Screenshot != null &&
                model.Screenshot.Length > 0)
            {
                var screenshot = model.Screenshot;

                if (screenshot.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        nameof(model.Screenshot),
                        "Ukuran screenshot maksimal 2 MB.");

                    return View("Create", model);
                }

                var ekstensi = Path
                    .GetExtension(screenshot.FileName)
                    .ToLowerInvariant();

                if (ekstensi != ".jpg" &&
                    ekstensi != ".jpeg" &&
                    ekstensi != ".png")
                {
                    ModelState.AddModelError(
                        nameof(model.Screenshot),
                        "Format screenshot harus JPG, JPEG, atau PNG.");

                    return View("Create", model);
                }

                try
                {
                    using var gambar = await Image.LoadAsync(
                        screenshot.OpenReadStream());

                    var format =
                        gambar.Metadata.DecodedImageFormat?.Name;

                    if (format != "JPEG" && format != "PNG")
                    {
                        ModelState.AddModelError(
                            nameof(model.Screenshot),
                            "Isi file harus berupa gambar JPG atau PNG.");

                        return View("Create", model);
                    }
                }
                catch (UnknownImageFormatException)
                {
                    ModelState.AddModelError(
                        nameof(model.Screenshot),
                        "File yang diunggah bukan gambar yang valid.");

                    return View("Create", model);
                }
                catch (InvalidImageContentException)
                {
                    ModelState.AddModelError(
                        nameof(model.Screenshot),
                        "Isi file gambar rusak atau tidak valid.");

                    return View("Create", model);
                }

                var namaFile = $"{Guid.NewGuid()}{ekstensi}";

                var folderUpload = Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "tiket");

                Directory.CreateDirectory(folderUpload);

                var lokasiFile = Path.Combine(
                    folderUpload,
                    namaFile);

                using (var stream = new FileStream(
                    lokasiFile, FileMode.Create))
                {
                    await screenshot.CopyToAsync(stream);
                }

                pathFile = $"/uploads/tiket/{namaFile}";
            }

            // Membuat nomor tiket publik yang unik
            string nomorTiket;

            do
            {
                var kodeAcak = Convert.ToHexString(RandomNumberGenerator.GetBytes(4));

                nomorTiket = $"HD-{DateTime.Now:yyyyMMdd}-{kodeAcak}";
            }
            while (await _context.Tikets.AnyAsync(
                t => t.NomorTiketPublik == nomorTiket));

            var kodePelacakan = Convert.ToHexString(RandomNumberGenerator.GetBytes(12));

            var kodeHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(kodePelacakan)));

            // Hanya masukkan field yang boleh diisi publik
            var tiket = new Tiket
            {
                Judul                  = model.Judul.Trim(),
                Keterangan             = model.Keterangan.Trim(),
                Status                 = "Baru",
                DateCreate             = DateTime.Now,
                DateUpdate             = null,
                ScreenshotPath         = pathFile,
                CreatedByUserId        = null,
                AssignedToUserId       = null,
                NomorTiketPublik       = nomorTiket,
                NamaPelapor            = model.NamaPelapor?.Trim(),
                NomorHpPelapor         = model.NomorHpPelapor?.Trim(),
                EmailPelapor           = model.EmailPelapor?.Trim(),
                KodePelacakanHash      = kodeHash,
                IzinkanPelacakanPublik = true
            };

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Simpan tiket baru
                _context.Tikets.Add(tiket);

                // Catat riwayat status awal
                var riwayat = new TiketStatusHistory
                {
                    Tiket            = tiket,
                    Status           = tiket.Status,
                    TanggalPerubahan = tiket.DateCreate,
                    Keterangan       = "Pengaduan berhasil diterima oleh sistem."
                };

                _context.TiketStatusHistories.Add(riwayat);

                // Simpan tiket dan riwayat dalam satu transaksi
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();

                // Bersihkan file jika penyimpanan gagal
                if (pathFile != null)
                {
                    var lokasiFile = Path.Combine(
                        _environment.WebRootPath,
                        pathFile.TrimStart('/')
                            .Replace('/', Path.DirectorySeparatorChar));

                    if (System.IO.File.Exists(lokasiFile))
                    {
                        System.IO.File.Delete(lokasiFile);
                    }
                }
                throw;
            }

            TempData["PublicNomorTiket"]    = nomorTiket;
            TempData["PublicKodePelacakan"] = kodePelacakan;
            TempData["PublicJudul"]         = tiket.Judul;
            TempData["PublicTanggalDibuat"] = tiket.DateCreate;

            return RedirectToAction(nameof(Created));
        }

        [HttpGet]
        public IActionResult Created()
        {
            var nomorTiket    = TempData["PublicNomorTiket"] as string;
            var kodePelacakan = TempData["PublicKodePelacakan"] as string;
            var judul         = TempData["PublicJudul"] as string;
            var tanggal       = TempData["PublicTanggalDibuat"];
            var tanggalDibuat = tanggal as DateTime?;

            if (string.IsNullOrEmpty(nomorTiket) ||
                string.IsNullOrEmpty(kodePelacakan) ||
                string.IsNullOrEmpty(judul) ||
                !tanggalDibuat.HasValue)
            {
                return RedirectToAction(nameof(Index));
            }

            var model = new PublicTiketCreatedViewModel
            {
                NomorTiketPublik = nomorTiket,
                KodePelacakan    = kodePelacakan,
                Judul            = judul,
                TanggalDibuat    = tanggalDibuat.Value
            };

            return View(model);
        }
    }
}
