using Microsoft.AspNetCore.Identity;

namespace Hepldesk.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string NamaLengkap { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}