﻿namespace EONIS.DTOs
{
    public class RegisterAdminDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public string? LicenseNumber { get; set; }
        public string? PharmacyName { get; set; }
        public bool IsSuperAdmin { get; set; } = false;
    }
}
