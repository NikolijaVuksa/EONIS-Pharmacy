using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EONIS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EONIS.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminUsersController(UserManager<ApplicationUser> userManager,
                                    RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public ActionResult<IEnumerable<object>> List()
        {
            var users = _userManager.Users.ToList();
            var result = users.Select(u => new {
                u.Id,
                u.UserName,
                u.Email,
                u.LockoutEnd,
                Roles = _userManager.GetRolesAsync(u).GetAwaiter().GetResult()
            });
            return Ok(result);
        }
    }
}
