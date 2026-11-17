using System.ComponentModel.DataAnnotations;

namespace Hepldesk.Models
{
    public class TiketStatusHistory
    {
        public int Id { get; set; }

        public int TiketId { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty;

        public DateTime TanggalPerubahan { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string? Keterangan { get; set; }

        // Relasi ke tiket
        public Tiket? Tiket { get; set; }
    }
}