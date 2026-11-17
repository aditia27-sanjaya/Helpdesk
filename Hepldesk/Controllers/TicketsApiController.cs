using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Hepldesk.Data;
using Hepldesk.Models;
using Hepldesk.Models.DTOs;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace Hepldesk.Controllers
{
    [ApiController]
    [Route("api/tickets")]
    [Authorize(Roles = "Admin,Petugas")]
    public class TicketsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public TicketsApiController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        // GET: api/tickets
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var query = _context.Tikets
                .Where(t => t.NomorTiketPublik == null);

            // Petugas hanya melihat tiket miliknya
            if (User.IsInRole("Petugas"))
            {
                var pengguna = await _userManager.GetUserAsync(User);

                if (pengguna == null)
                {
                    return Unauthorized();
                }

                query = query.Where(t =>
                    t.AssignedToUserId == pengguna.Id);
            }

            var tikets = await query
                .OrderByDescending(t => t.DateCreate)
                .Select(t => new TiketResponseDto
                {
                    Id = t.Id,
                    Judul = t.Judul,
                    Keterangan = t.Keterangan,
                    Status = t.Status,
                    DateCreate = t.DateCreate,
                    DateUpdate = t.DateUpdate,
                    ScreenshotPath = t.ScreenshotPath,
                    AssignedToUserId = t.AssignedToUserId
                })
                .ToListAsync();

            return Ok(tikets);
        }

        // GET: api/tickets/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var query = _context.Tikets
                .Where(t => t.NomorTiketPublik == null);

            // Petugas hanya dapat melihat tiket yang ditugaskan kepadanya
            if (User.IsInRole("Petugas"))
            {
                var pengguna = await _userManager.GetUserAsync(User);

                if (pengguna == null)
                {
                    return Unauthorized();
                }

                query = query.Where(t =>
                    t.AssignedToUserId == pengguna.Id);
            }

            var tiket = await query
                .Where(t => t.Id == id)
                .Select(t => new TiketResponseDto
                {
                    Id = t.Id,
                    Judul = t.Judul,
                    Keterangan = t.Keterangan,
                    Status = t.Status,
                    DateCreate = t.DateCreate,
                    DateUpdate = t.DateUpdate,
                    ScreenshotPath = t.ScreenshotPath,
                    AssignedToUserId = t.AssignedToUserId
                })
                .FirstOrDefaultAsync();

            if (tiket == null)
            {
                return NotFound(new
                {
                    message = "Tiket tidak ditemukan."
                });
            }

            return Ok(tiket);
        }

        // POST: api/tickets
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateTiketRequestDto model)
        {
            // Memastikan Admin yang sedang login tersedia
            var admin = await _userManager.GetUserAsync(User);

            if (admin == null)
            {
                return Unauthorized();
            }

            // Validasi petugas yang akan menerima penugasan
            if (model.AssignedToUserId.HasValue)
            {
                var petugas = await _userManager.FindByIdAsync(
                    model.AssignedToUserId.Value.ToString());

                if (petugas == null ||
                    !petugas.IsActive ||
                    !await _userManager.IsInRoleAsync(petugas, "Petugas"))
                {
                    return BadRequest(new
                    {
                        message = "Petugas yang dipilih tidak valid atau tidak aktif."
                    });
                }
            }

            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var tiket = new Tiket
                {
                    Judul = model.Judul,
                    Keterangan = model.Keterangan,
                    Status = "Baru",
                    DateCreate = DateTime.Now,
                    DateUpdate = null,
                    CreatedByUserId = admin.Id,
                    AssignedToUserId = model.AssignedToUserId
                };

                _context.Tikets.Add(tiket);
                await _context.SaveChangesAsync();

                // Membuat riwayat status awal
                var riwayat = new TiketStatusHistory
                {
                    TiketId = tiket.Id,
                    Status = "Baru",
                    TanggalPerubahan = DateTime.Now,
                    Keterangan = "Tiket berhasil dibuat melalui API oleh Admin."
                };

                _context.TiketStatusHistories.Add(riwayat);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                var response = new TiketResponseDto
                {
                    Id = tiket.Id,
                    Judul = tiket.Judul,
                    Keterangan = tiket.Keterangan,
                    Status = tiket.Status,
                    DateCreate = tiket.DateCreate,
                    DateUpdate = tiket.DateUpdate,
                    ScreenshotPath = tiket.ScreenshotPath,
                    AssignedToUserId = tiket.AssignedToUserId
                };

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = tiket.Id },
                    response);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // PUT: api/tickets/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            UpdateTiketRequestDto model)
        {
            // Ambil tiket internal
            var tiket = await _context.Tikets
                .FirstOrDefaultAsync(t =>
                    t.Id == id &&
                    t.NomorTiketPublik == null);

            if (tiket == null)
            {
                return NotFound(new
                {
                    message = "Tiket tidak ditemukan."
                });
            }

            // Petugas hanya boleh mengubah tiket miliknya
            if (User.IsInRole("Petugas"))
            {
                var petugas = await _userManager.GetUserAsync(User);

                if (petugas == null)
                {
                    return Unauthorized();
                }

                if (tiket.AssignedToUserId != petugas.Id)
                {
                    return NotFound(new
                    {
                        message = "Tiket tidak ditemukan."
                    });
                }
            }

            // Simpan status lama untuk mengetahui perubahan
            string statusLama = tiket.Status;

            bool statusBerubah = statusLama != model.Status;

            // Jika status berubah, keterangan wajib diisi
            if (statusBerubah &&
                string.IsNullOrWhiteSpace(model.KeteranganStatus))
            {
                return BadRequest(new
                {
                    message = "Keterangan perubahan status wajib diisi."
                });
            }

            // Hanya Admin yang boleh mengubah penugasan
            if (User.IsInRole("Admin") &&
                model.AssignedToUserId.HasValue)
            {
                var petugas = await _userManager.FindByIdAsync(
                    model.AssignedToUserId.Value.ToString());

                if (petugas == null ||
                    !petugas.IsActive ||
                    !await _userManager.IsInRoleAsync(petugas, "Petugas"))
                {
                    return BadRequest(new
                    {
                        message = "Petugas yang dipilih tidak valid atau tidak aktif."
                    });
                }
            }

            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                tiket.Judul = model.Judul;
                tiket.Keterangan = model.Keterangan;
                tiket.Status = model.Status;
                tiket.DateUpdate = DateTime.Now;

                // Perubahan penugasan hanya untuk Admin
                if (User.IsInRole("Admin"))
                {
                    tiket.AssignedToUserId = model.AssignedToUserId;
                }

                // Catat riwayat hanya jika status berubah
                if (statusBerubah)
                {
                    var riwayat = new TiketStatusHistory
                    {
                        TiketId = tiket.Id,
                        Status = model.Status,
                        TanggalPerubahan = DateTime.Now,
                        Keterangan = model.KeteranganStatus!.Trim()
                    };

                    _context.TiketStatusHistories.Add(riwayat);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // DELETE: api/tickets/{id}
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            // Cari tiket internal
            var tiket = await _context.Tikets
                .FirstOrDefaultAsync(t =>
                    t.Id == id &&
                    t.NomorTiketPublik == null);

            if (tiket == null)
            {
                return NotFound(new
                {
                    message = "Tiket tidak ditemukan."
                });
            }

            // Simpan lokasi screenshot sebelum tiket dihapus
            string? screenshotPath = tiket.ScreenshotPath;

            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Tikets.Remove(tiket);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Hapus file screenshot setelah transaksi berhasil
                if (!string.IsNullOrWhiteSpace(screenshotPath))
                {
                    string namaFile = Path.GetFileName(screenshotPath);

                    string folderUpload = Path.Combine(
                        _environment.WebRootPath,
                        "uploads",
                        "tiket");

                    string lokasiFile = Path.Combine(
                        folderUpload,
                        namaFile);

                    if (System.IO.File.Exists(lokasiFile))
                    {
                        System.IO.File.Delete(lokasiFile);
                    }
                }

                return NoContent();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}