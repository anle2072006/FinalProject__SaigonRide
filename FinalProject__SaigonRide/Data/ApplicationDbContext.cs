using FinalProject__SaigonRide.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinalProject__SaigonRide.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Station> Stations { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Coupon> Coupons { get; set; }

        public DbSet<TransactionHistory> TransactionHistories { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Coupon>().HasData(
                new Coupon { Id = 1, CodeName = "WELCOME2026", DiscountValue = 20000, IsActive = true },
                new Coupon { Id = 2, CodeName = "SAIGONRIDE10", DiscountValue = 10000, IsActive = true },
                new Coupon { Id = 3, CodeName = "TESTCOUPON", DiscountValue = 5000, IsActive = false }
            );
        }
    }
}