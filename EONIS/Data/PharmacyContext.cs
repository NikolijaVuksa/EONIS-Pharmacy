using EONIS.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EONIS.Data
{
    public class PharmacyContext : IdentityDbContext<ApplicationUser>
    {
        public PharmacyContext(DbContextOptions<PharmacyContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<StockBatch> StockBatches => Set<StockBatch>();

        public DbSet<Order> Orders => Set<Order>();         
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        public DbSet<CustomerProfile> Customers => Set<CustomerProfile>();
        public DbSet<AdminProfile> Admins => Set<AdminProfile>();
        public DbSet<Payment> Payments { get; set; } = default!;

        public DbSet<PrescriptionReservation> Reservations => Set<PrescriptionReservation>();
        public DbSet<Therapy> Therapies => Set<Therapy>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .Property(p => p.BasePrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<StockBatch>()
                .HasOne(sb => sb.Product)
                .WithMany(p => p.StockBatches)
                .HasForeignKey(sb => sb.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StockBatch>()
                .HasIndex(sb => new { sb.ProductId, sb.LotNumber })
                .IsUnique();

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

            modelBuilder.Entity<PrescriptionReservation>()
                .HasOne(r => r.Product)
                .WithMany()
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PrescriptionReservation>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Therapy>()
                .HasOne(t => t.Product)
                .WithMany()
                .HasForeignKey(t => t.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Therapy>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Reservation)
                .WithMany()
                .HasForeignKey(o => o.ReservationId)
                .OnDelete(DeleteBehavior.SetNull);



        }
    }
}
