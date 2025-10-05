namespace EONIS.Models
{
    public class CustomerProfile
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        public string? Address { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? InsuranceNumber { get; set; }
    }
}
