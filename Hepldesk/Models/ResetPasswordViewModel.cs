using System.ComponentModel.DataAnnotations;

namespace Hepldesk.Models
{
    public class ResetPasswordViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Password baru wajib diisi.")]
        [StringLength(100, MinimumLength = 8,
            ErrorMessage = "Password harus 8–100 karakter.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password Baru")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Konfirmasi password wajib diisi.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password),
            ErrorMessage = "Konfirmasi password tidak cocok.")]
        [Display(Name = "Konfirmasi Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}