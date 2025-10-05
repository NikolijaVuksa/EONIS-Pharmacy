using Microsoft.AspNetCore.Identity;

namespace EONIS.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public CustomerProfile? CustomerProfile { get; set; }
        public AdminProfile? AdminProfile { get; set; }
    }
}
