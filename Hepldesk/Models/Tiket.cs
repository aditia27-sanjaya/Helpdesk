
using System.ComponentModel.DataAnnotations;

namespace Hepldesk.Models
{
    public class Tiket
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Judul tiket wajib diisi.")]
        [StringLength(100, ErrorMessage = "Judul maksimal 100 karakter.")]
        public string Judul { get; set; } = string.Empty;

        [Required(ErrorMessage = "Keterangan wajib diisi.")]
        [StringLength(2000, ErrorMessage = "Keterangan maksimal 2000 karakter.")]
        public string Keterangan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status wajib dipilih.")]
        [RegularExpression("^(Baru|Diproses|Selesai)$",
            ErrorMessage = "Status tiket tidak valid.")]
        public string Status { get; set; } = "Baru";

        public DateTime DateCreate { get; set; } = DateTime.Now;

        public DateTime? DateUpdate { get; set; }

        public string? ScreenshotPath { get; set; }

        public int? CreatedByUserId { get; set; }

        public int? AssignedToUserId { get; set; }

        // Informasi pengaduan publik
        [StringLength(32)]
        public string? NomorTiketPublik { get; set; }

        [StringLength(150)]
        public string? NamaPelapor { get; set; }

        [Phone(ErrorMessage = "Format nomor HP tidak valid.")]
        [StringLength(30)]
        public string? NomorHpPelapor { get; set; }

        [EmailAddress(ErrorMessage = "Format email tidak valid.")]
        [StringLength(254)]
        public string? EmailPelapor { get; set; }

        // Hash kode rahasia, bukan kode aslinya
        [StringLength(128)]
        public string? KodePelacakanHash { get; set; }

        // Tiket tidak dapat dilacak publik secara default
        public bool IzinkanPelacakanPublik { get; set; } = false;
    }
}
