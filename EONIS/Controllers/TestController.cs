using Microsoft.AspNetCore.Mvc;

namespace EONIS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("Backend radi");
        }
    }
}
