
namespace Hepldesk.Models
{
    public class PublicTiketCreatedViewModel
    {
        public string NomorTiketPublik { get; set; } = string.Empty;

        public string KodePelacakan { get; set; } = string.Empty;

        public string Judul { get; set; } = string.Empty;

        public DateTime TanggalDibuat { get; set; }
    }
}
