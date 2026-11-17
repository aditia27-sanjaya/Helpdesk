using System.ComponentModel.DataAnnotations;

namespace Hepldesk.Models.DTOs
{
    public class CreateTiketRequestDto
    {
        [Required(ErrorMessage = "Judul tiket wajib diisi.")]
        [StringLength(100, ErrorMessage = "Judul maksimal 100 karakter.")]
        public string Judul { get; set; } = string.Empty;

        [Required(ErrorMessage = "Keterangan wajib diisi.")]
        [StringLength(2000, ErrorMessage = "Keterangan maksimal 2000 karakter.")]
        public string Keterangan { get; set; } = string.Empty;

        public int? AssignedToUserId { get; set; }
    }
}