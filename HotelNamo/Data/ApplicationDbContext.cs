using HotelNamo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HotelNamo.Data
{

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<HousekeepingTask> HousekeepingTasks { get; set; }
        public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }
        public DbSet<Amenity> Amenities { get; set; }
        public DbSet<RoomAmenity> RoomAmenities { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<RoomImage> RoomImages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);  // <--- IMPORTANT: Don't remove this!

            builder.Entity<MaintenanceRequest>()
                .HasOne(m => m.AssignedStaff)
                .WithMany()
                .HasForeignKey(m => m.AssignedStaffId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<HousekeepingTask>()
                .HasOne(ht => ht.Room)
                .WithMany(r => r.HousekeepingTasks)
                .HasForeignKey(ht => ht.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<HousekeepingTask>()
                .HasOne(ht => ht.AssignedStaff)
                .WithMany()
                .HasForeignKey(ht => ht.AssignedStaffId)
                .OnDelete(DeleteBehavior.SetNull);  // ✅ Fix: Set null instead of requiring a value


            builder.Entity<Feedback>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            // Explicitly define composite primary key clearly:
            builder.Entity<RoomAmenity>()
                .HasKey(ra => new { ra.RoomId, ra.AmenityId });

            // Explicitly define relationships clearly:
            builder.Entity<RoomAmenity>()
                .HasOne(ra => ra.Room)
                .WithMany(r => r.RoomAmenities)
                .HasForeignKey(ra => ra.RoomId);

            builder.Entity<RoomAmenity>()
                .HasOne(ra => ra.Amenity)
                .WithMany(a => a.RoomAmenities)
                .HasForeignKey(ra => ra.AmenityId);
            builder.Entity<Booking>()
                .HasOne(b => b.Room)
                .WithMany(r => r.Bookings)  // **This was missing!**
                .HasForeignKey(b => b.RoomId);
        }

    }

}
