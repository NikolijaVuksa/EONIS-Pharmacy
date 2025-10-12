using EONIS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminUsersController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public ActionResult<IEnumerable<object>> GetAll()
    {
        var users = _userManager.Users.ToList();
        var result = users.Select(u => new {
            u.Id,
            u.UserName,
            u.Email,
            FullName = u.FullName,
            Roles = _userManager.GetRolesAsync(u).GetAwaiter().GetResult()
        });
        return Ok(result);
    }
}
