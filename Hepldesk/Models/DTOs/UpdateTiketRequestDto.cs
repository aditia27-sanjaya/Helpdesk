using System.ComponentModel.DataAnnotations;

namespace Hepldesk.Models.DTOs
{
    public class UpdateTiketRequestDto
    {
        [Required(ErrorMessage = "Judul tiket wajib diisi.")]
        [StringLength(100, ErrorMessage = "Judul maksimal 100 karakter.")]
        public string Judul { get; set; } = string.Empty;

        [Required(ErrorMessage = "Keterangan wajib diisi.")]
        [StringLength(2000, ErrorMessage = "Keterangan maksimal 2000 karakter.")]
        public string Keterangan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status wajib dipilih.")]
        [RegularExpression("^(Baru|Diproses|Selesai)$",
            ErrorMessage = "Status tiket tidak valid.")]
        public string Status { get; set; } = string.Empty;

        [StringLength(500,
            ErrorMessage = "Keterangan perubahan status maksimal 500 karakter.")]
        public string? KeteranganStatus { get; set; }

        public int? AssignedToUserId { get; set; }
    }
}