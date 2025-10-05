using EONIS.Data;
using EONIS.DTOs;
using EONIS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EONIS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfilesController : ControllerBase
    {
        private readonly PharmacyContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfilesController(PharmacyContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [Authorize(Roles = "Customer,Admin")]
        [HttpPut("customer")]
        public async Task<IActionResult> UpdateCustomer([FromBody] CustomerProfileDto dto)
        {
            var userEmail = User.Identity?.Name;
            if (userEmail == null) return Unauthorized();

            var user = await _userManager.FindByNameAsync(userEmail);
            if (user == null) return NotFound("User not found");

            var profile = await _db.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (profile == null) return NotFound("Profile not found");

            profile.Address = dto.Address;
            profile.City = dto.City;
            profile.PostalCode = dto.PostalCode;
            profile.DateOfBirth = dto.DateOfBirth;
            profile.InsuranceNumber = dto.InsuranceNumber;

            await _db.SaveChangesAsync();
            return Ok("Customer profile updated");
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("admin")]
        public async Task<IActionResult> UpdateAdmin([FromBody] AdminProfileDto dto)
        {
            var userEmail = User.Identity?.Name;
            if (userEmail == null) return Unauthorized();

            var user = await _userManager.FindByNameAsync(userEmail);
            if (user == null) return NotFound("User not found");

            var profile = await _db.Admins.FirstOrDefaultAsync(a => a.UserId == user.Id);
            if (profile == null) return NotFound("Profile not found");

            profile.LicenseNumber = dto.LicenseNumber;
            profile.PharmacyName = dto.PharmacyName;
            profile.IsSuperAdmin = dto.IsSuperAdmin;

            await _db.SaveChangesAsync();
            return Ok("Admin profile updated");
        }
    }
}
