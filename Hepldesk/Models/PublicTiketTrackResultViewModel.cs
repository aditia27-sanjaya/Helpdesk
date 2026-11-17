
namespace Hepldesk.Models
{
    public class PublicTiketTrackResultViewModel
    {
        // Informasi tiket
        public string NomorTiketPublik { get; set; } = string.Empty;

        public string Judul { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime TanggalDibuat { get; set; }

        // Daftar riwayat perubahan status
        public List<PublicTiketStatusHistoryViewModel> RiwayatStatus
        { get; set; } = new();
    }

    // Informasi riwayat yang boleh dilihat pelapor
    public class PublicTiketStatusHistoryViewModel
    {
        public string Status { get; set; } = string.Empty;

        public DateTime TanggalPerubahan { get; set; }

        public string? Keterangan { get; set; }
    }
}
