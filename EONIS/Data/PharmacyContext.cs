using EONIS.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EONIS.Data
{
    public class PharmacyContext : IdentityDbContext<ApplicationUser>
    {
        public PharmacyContext(DbContextOptions<PharmacyContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();

        public DbSet<Order> Orders => Set<Order>();         
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        public DbSet<CustomerProfile> Customers => Set<CustomerProfile>();
        public DbSet<AdminProfile> Admins => Set<AdminProfile>();
        public DbSet<Payment> Payments { get; set; } = default!;

        public DbSet<StripeEventLog> StripeEvents => Set<StripeEventLog>();



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .Property(p => p.BasePrice)
                .HasColumnType("decimal(18,2)");


            modelBuilder.Entity<OrderItem>()
                .HasOne(i => i.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId);

            modelBuilder.Entity<CustomerProfile>()
               .HasOne(c => c.User)
               .WithOne(u => u.CustomerProfile)
               .HasForeignKey<CustomerProfile>(c => c.UserId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AdminProfile>()
                .HasOne(a => a.User)
                .WithOne(u => u.AdminProfile)
                .HasForeignKey<AdminProfile>(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
