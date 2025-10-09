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
    public class TherapiesController : ControllerBase
    {
        private readonly PharmacyContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public TherapiesController(PharmacyContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // kreira terapiju iz recepta
        [Authorize(Roles = "Customer")]
        [HttpPost("from-prescription")]
        public async Task<ActionResult<TherapyReadDto>> CreateFromPrescription([FromBody] TherapyFromPrescriptionDto dto)
        {
            var email = User.Identity?.Name;
            if (email == null) return Unauthorized();

            var me = await _userManager.FindByNameAsync(email);
            if (me == null) return NotFound("User not found");

            var product = await _db.Products.FindAsync(dto.ProductId);
            if (product == null) return NotFound("Product not found");
            if (!product.Rx) return BadRequest("Therapy can be created only for Rx products.");

            var t = new Therapy
            {
                UserId = me.Id,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                Dosage = dto.Dosage,
                DosageUnit = dto.DosageUnit,
                Frequency = dto.Frequency,
                DurationDays = dto.DurationDays,
                PrescriptionCode = dto.PrescriptionCode,
                DoctorName = dto.DoctorName,
                Status = "Pending"
            };

            _db.Therapies.Add(t);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = t.Id }, new TherapyReadDto
            {
                Id = t.Id,
                ProductId = t.ProductId,
                ProductName = product.Name,
                Quantity = t.Quantity,
                Dosage = t.Dosage,
                DosageUnit = t.DosageUnit,
                Frequency = t.Frequency,
                DurationDays = t.DurationDays,
                PrescriptionCode = t.PrescriptionCode,
                DoctorName = t.DoctorName,
                Status = t.Status,
                CreatedAt = t.CreatedAt
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
                Quantity = t.Quantity,
                Dosage = t.Dosage,
                DosageUnit = t.DosageUnit,
                Frequency = t.Frequency,
                DurationDays = t.DurationDays,
                PrescriptionCode = t.PrescriptionCode,
                DoctorName = t.DoctorName,
                Status = t.Status,
                CreatedAt = t.CreatedAt
            });
        }

        // kad admin odobri kreira se order
        [Authorize(Roles = "Admin")]
        [HttpPost("{id:int}/approve")]
        public async Task<IActionResult> ApproveAndCreateOrder(int id)
        {
            var therapy = await _db.Therapies
                .Include(t => t.Product)
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (therapy == null) return NotFound("Therapy not found.");
            if (therapy.Status == "Approved") return BadRequest("Therapy already approved.");
            if (therapy.Product == null || !therapy.Product.Rx) return BadRequest("Only Rx therapies can be approved.");

            therapy.Status = "Approved";

         
            var order = new Order
            {
                CustomerEmail = therapy.User!.Email,
                CreatedAt = DateTime.UtcNow,
                Status = "Created",
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = therapy.ProductId,
                        Quantity = therapy.Quantity,
                        UnitPrice = therapy.Product.PriceWithVat
                    }
                }
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Therapy approved. Order created for the user.",
                therapyId = therapy.Id,
                orderId = order.Id
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id:int}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            var therapy = await _db.Therapies.FindAsync(id);
            if (therapy == null) return NotFound();
            if (therapy.Status == "Approved") return BadRequest("Cannot reject an approved therapy.");

            therapy.Status = "Rejected";
            await _db.SaveChangesAsync();
            return Ok();
        }

        [Authorize(Roles = "Customer")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TherapyReadDto>>> GetMyTherapies()
        {
            var email = User.Identity?.Name;
            if (email == null) return Unauthorized();
            var me = await _userManager.FindByNameAsync(email);
            if (me == null) return NotFound("User not found");

            var list = await _db.Therapies.Include(t => t.Product)
                .Where(t => t.UserId == me.Id)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Ok(list.Select(t => new TherapyReadDto
            {
                Id = t.Id,
                ProductId = t.ProductId,
                ProductName = t.Product?.Name ?? string.Empty,
                Quantity = t.Quantity,
                Dosage = t.Dosage,
                DosageUnit = t.DosageUnit,
                Frequency = t.Frequency,
                DurationDays = t.DurationDays,
                PrescriptionCode = t.PrescriptionCode,
                DoctorName = t.DoctorName,
                Status = t.Status,
                CreatedAt = t.CreatedAt
            }));
        }
    }
}
