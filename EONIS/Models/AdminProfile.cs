namespace EONIS.Models
{
    public class AdminProfile
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        public string? LicenseNumber { get; set; }
        public string? PharmacyName { get; set; }
        public bool IsSuperAdmin { get; set; } = false;
    }
}
