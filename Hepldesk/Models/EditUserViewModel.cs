
using System.ComponentModel.DataAnnotations;

namespace Hepldesk.Models
{
    public class EditUserViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Nama lengkap wajib diisi.")]
        [StringLength(100, ErrorMessage = "Nama maksimal 100 karakter.")]
        [Display(Name = "Nama Lengkap")]
        public string NamaLengkap { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username wajib diisi.")]
        [StringLength(50, MinimumLength = 4,
            ErrorMessage = "Username harus 4–50 karakter.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email wajib diisi.")]
        [EmailAddress(ErrorMessage = "Format email tidak valid.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role pengguna wajib dipilih.")]
        [Display(Name = "Role Pengguna")]
        public string Role { get; set; } = string.Empty;

        [Display(Name = "Status Akun")]
        public bool IsActive { get; set; } = true;
    }
}
