using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class TherapiesController : ControllerBase
    {
        private readonly PharmacyContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public TherapiesController(PharmacyContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<ActionResult<TherapyReadDto>> Create([FromBody] TherapyCreateDto dto)
        {
            var email = User.Identity?.Name;
            if (email == null) return Unauthorized();
            var user = await _userManager.FindByNameAsync(email);
            if (user == null) return NotFound("User not found");

            var product = await _db.Products.FindAsync(dto.ProductId);
            if (product == null) return NotFound("Product not found");

            var t = new Therapy
            {
                UserId = user.Id,
                ProductId = dto.ProductId,
                Dosage = dto.Dosage,
                Frequency = dto.Frequency,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate
            };

            _db.Therapies.Add(t);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = t.Id }, new TherapyReadDto
            {
                Id = t.Id,
                ProductId = t.ProductId,
                ProductName = product.Name,
                Dosage = t.Dosage,
                Frequency = t.Frequency,
                StartDate = t.StartDate,
                EndDate = t.EndDate
            });
        }

        [Authorize(Roles = "Customer,Admin")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TherapyReadDto>> Get(int id)
        {
            var t = await _db.Therapies.Include(x => x.Product).FirstOrDefaultAsync(x => x.Id == id);
            if (t == null) return NotFound();

            if (!User.IsInRole("Admin"))
            {
                var email = User.Identity?.Name;
                if (email == null) return Unauthorized();
                var me = await _userManager.FindByNameAsync(email);
                if (me == null || me.Id != t.UserId) return Forbid();
            }

            return Ok(new TherapyReadDto
            {
                Id = t.Id,
                ProductId = t.ProductId,
                ProductName = t.Product?.Name ?? string.Empty,
                Dosage = t.Dosage,
                Frequency = t.Frequency,
                StartDate = t.StartDate,
                EndDate = t.EndDate
            });
        }

        [Authorize(Roles = "Customer")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TherapyReadDto>>> GetMyTherapies()
        {
            var email = User.Identity?.Name;
            if (email == null) return Unauthorized();
            var me = await _userManager.FindByNameAsync(email);
            if (me == null) return NotFound("User not found");

            var list = await _db.Therapies
                .Include(t => t.Product)
                .Where(t => t.UserId == me.Id)
                .OrderByDescending(t => t.StartDate)
                .ToListAsync();

            return Ok(list.Select(t => new TherapyReadDto
            {
                Id = t.Id,
                ProductId = t.ProductId,
                ProductName = t.Product?.Name ?? string.Empty,
                Dosage = t.Dosage,
                Frequency = t.Frequency,
                StartDate = t.StartDate,
                EndDate = t.EndDate
            }));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<TherapyReadDto>>> ListAll()
        {
            var list = await _db.Therapies
                .Include(t => t.Product)
                .Include(t => t.User)
                .OrderByDescending(t => t.StartDate)
                .ToListAsync();

            return Ok(list.Select(t => new TherapyReadDto
            {
                Id = t.Id,
                ProductId = t.ProductId,
                ProductName = t.Product?.Name ?? string.Empty,
                Dosage = t.Dosage,
                Frequency = t.Frequency,
                StartDate = t.StartDate,
                EndDate = t.EndDate
            }));
        }
    }
}
