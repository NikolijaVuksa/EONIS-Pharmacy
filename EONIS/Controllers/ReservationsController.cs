using System;
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
    public class ReservationsController : ControllerBase
    {
        private readonly PharmacyContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReservationsController(PharmacyContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        //rez leka
        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<ActionResult<ReservationReadDto>> Create([FromBody] ReservationCreateDto dto)
        {
            var email = User.Identity?.Name;
            if (email == null) return Unauthorized();
            var user = await _userManager.FindByNameAsync(email);
            if (user == null) return NotFound("User not found");

            var product = await _db.Products.FindAsync(dto.ProductId);
            if (product == null) return NotFound("Product not found");
            if (!product.Rx) return BadRequest("Only prescription drugs can be reserved.");

            var reservation = new PrescriptionReservation
            {
                ProductId = dto.ProductId,
                UserId = user.Id,
                Quantity = dto.Quantity,
                PrescriptionCode = dto.PrescriptionCode,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _db.Reservations.Add(reservation);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = reservation.Id }, new ReservationReadDto
            {
                Id = reservation.Id,
                ProductId = reservation.ProductId,
                ProductName = product.Name,
                Quantity = reservation.Quantity,
                PrescriptionCode = reservation.PrescriptionCode,
                Status = reservation.Status,
                CreatedAt = reservation.CreatedAt
            });
        }

        //rez po id
        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ReservationReadDto>> Get(int id)
        {
            var r = await _db.Reservations
                .Include(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (r == null) return NotFound();

            // vlasnik ili admin
            var email = User.Identity?.Name;
            if (email == null) return Unauthorized();
            var me = await _userManager.FindByNameAsync(email);
            var isAdmin = me != null && await _userManager.IsInRoleAsync(me, "Admin");
            if (!isAdmin && r.UserId != me?.Id) return Forbid();

            return Ok(new ReservationReadDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                ProductName = r.Product?.Name ?? string.Empty,
                Quantity = r.Quantity,
                PrescriptionCode = r.PrescriptionCode,
                Status = r.Status,
                CreatedAt = r.CreatedAt
            });
        }

        [Authorize(Roles = "Customer")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservationReadDto>>> GetMyReservations()
        {
            var email = User.Identity?.Name;
            if (email == null) return Unauthorized();
            var me = await _userManager.FindByNameAsync(email);
            if (me == null) return NotFound("User not found");

            var list = await _db.Reservations
                .Include(r => r.Product)
                .Where(r => r.UserId == me.Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(list.Select(r => new ReservationReadDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                ProductName = r.Product?.Name ?? string.Empty,
                Quantity = r.Quantity,
                PrescriptionCode = r.PrescriptionCode,
                Status = r.Status,
                CreatedAt = r.CreatedAt
            }));
        }

        // sve rez sa ili bez statusa 
        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<ReservationReadDto>>> ListAll([FromQuery] string? status = null)
        {
            var q = _db.Reservations.Include(r => r.Product).AsQueryable();
            if (!string.IsNullOrWhiteSpace(status))
                q = q.Where(r => r.Status == status);

            var list = await q.OrderByDescending(r => r.CreatedAt).ToListAsync();

            return Ok(list.Select(r => new ReservationReadDto
            {
                Id = r.Id,
                ProductId = r.ProductId,
                ProductName = r.Product?.Name ?? string.Empty,
                Quantity = r.Quantity,
                PrescriptionCode = r.PrescriptionCode,
                Status = r.Status,
                CreatedAt = r.CreatedAt
            }));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id:int}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var r = await _db.Reservations.FirstOrDefaultAsync(x => x.Id == id);
            if (r == null) return NotFound();
            r.Status = "Approved";
            await _db.SaveChangesAsync();
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id:int}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            var r = await _db.Reservations.FirstOrDefaultAsync(x => x.Id == id);
            if (r == null) return NotFound();
            r.Status = "Rejected";
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}
