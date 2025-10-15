using EONIS.Data;
using Microsoft.EntityFrameworkCore;

namespace EONIS.Services
{
    public class OrderService
    {
        private readonly PharmacyContext _db;
        public OrderService(PharmacyContext db) => _db = db;


        

    }
}
