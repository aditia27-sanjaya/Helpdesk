
using System.ComponentModel.DataAnnotations;

namespace Hepldesk.Models
{
    public class PublicTiketTrackViewModel
    {
        [Required(ErrorMessage = "Nomor tiket wajib diisi.")]
        [StringLength(32)]
        public string NomorTiketPublik { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kode pelacakan wajib diisi.")]
        [StringLength(24, MinimumLength = 24,
            ErrorMessage = "Kode pelacakan harus terdiri dari 24 karakter.")]
        public string KodePelacakan { get; set; } = string.Empty;
    }
}
