using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EONIS.Data;
using EONIS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EONIS.Controllers
{
    [ApiController]
    [Route("api/admin/payments")]
    [Authorize(Roles = "Admin")]
    public class AdminPaymentsController : ControllerBase
    {
        private readonly PharmacyContext _db;

        public AdminPaymentsController(PharmacyContext db)
        {
            _db = db;
        }

        // /admin/payments?status=Succeeded&from=2025-01-01&to=2025-12-31
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Payment>>> GetAll(
            [FromQuery] string? status = null,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var q = _db.Payments.AsQueryable();
            if (!string.IsNullOrWhiteSpace(status))
                q = q.Where(p => p.Status == status);
            if (from.HasValue)
                q = q.Where(p => p.CreatedAt >= from.Value);
            if (to.HasValue)
                q = q.Where(p => p.CreatedAt <= to.Value);

            var list = await q.OrderByDescending(p => p.CreatedAt).ToListAsync();
            return Ok(list);
        }

        // /admin/payments/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Payment>> Get(int id)
        {
            var p = await _db.Payments.FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) return NotFound();
            return Ok(p);
        }
    }
}
