namespace EONIS.DTOs
{
    public class CreateUserDto
    {
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "Customer";
    }

    public class UpdateRoleDto
    {
        public string Role { get; set; } = "Customer";
    }
}
