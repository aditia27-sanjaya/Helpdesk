
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Hepldesk.Models
{
    public class PublicTiketViewModel
    {
        [Required(ErrorMessage = "Judul keluhan wajib diisi.")]
        [StringLength(100, ErrorMessage = "Judul maksimal 100 karakter.")]
        [Display(Name = "Judul Keluhan")]
        public string Judul { get; set; } = string.Empty;

        [Required(ErrorMessage = "Keterangan keluhan wajib diisi.")]
        [StringLength(2000, ErrorMessage = "Keterangan maksimal 2000 karakter.")]
        [Display(Name = "Deskripsi Keluhan")]
        public string Keterangan { get; set; } = string.Empty;

        [StringLength(150, ErrorMessage = "Nama maksimal 150 karakter.")]
        [Display(Name = "Nama Pelapor (Opsional)")]
        public string? NamaPelapor { get; set; }

        [Phone(ErrorMessage = "Format nomor HP tidak valid.")]
        [StringLength(30, ErrorMessage = "Nomor HP maksimal 30 karakter.")]
        [Display(Name = "Nomor HP (Opsional)")]
        public string? NomorHpPelapor { get; set; }

        [EmailAddress(ErrorMessage = "Format email tidak valid.")]
        [StringLength(254, ErrorMessage = "Email maksimal 254 karakter.")]
        [Display(Name = "Email (Opsional)")]
        public string? EmailPelapor { get; set; }

        [Display(Name = "Screenshot (Opsional, JPG/PNG, maksimal 2 MB)")]
        public IFormFile? Screenshot { get; set; }
    }
}
