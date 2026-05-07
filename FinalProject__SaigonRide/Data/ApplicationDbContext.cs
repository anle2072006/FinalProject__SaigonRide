using FinalProject__SaigonRide.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using static Azure.Core.HttpHeader;
using static System.Collections.Specialized.BitVector32;

namespace FinalProject__SaigonRide.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Station> Stations { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        // ... các bảng khác

        // THÊM ĐOẠN NÀY ĐỂ SEED DATA
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Thêm sẵn 3 mã giảm giá
            modelBuilder.Entity<Coupon>().HasData(
                new Coupon { Id = 1, CodeName = "WELCOME2026", DiscountValue = 20000, IsActive = true },
                new Coupon { Id = 2, CodeName = "SAIGONRIDE10", DiscountValue = 10000, IsActive = true },
                new Coupon { Id = 3, CodeName = "TESTCOUPON", DiscountValue = 5000, IsActive = false }
            );

            // Bạn cũng có thể làm tương tự cho bảng Station
            // modelBuilder.Entity<Station>().HasData(
            //     new Station { Id = 1, Name = "Ben Thanh", Location = "Quận 1" }
            // );
        }
    }
}
