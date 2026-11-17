namespace Hepldesk.Models.DTOs
{
    public class TiketResponseDto
    {
        public int Id { get; set; }

        public string Judul { get; set; } = string.Empty;

        public string Keterangan { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime DateCreate { get; set; }

        public DateTime? DateUpdate { get; set; }

        public string? ScreenshotPath { get; set; }

        public int? AssignedToUserId { get; set; }
    }
}