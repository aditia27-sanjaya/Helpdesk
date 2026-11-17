using Hepldesk.Models;

namespace Hepldesk.Models
{
    public class UserListViewModel
    {
        public ApplicationUser User { get; set; } = null!;

        public IList<string> Roles { get; set; } = new List<string>();
    }
}