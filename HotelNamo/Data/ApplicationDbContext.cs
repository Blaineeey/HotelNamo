using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Room> Rooms { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<ServiceRequest> ServiceRequests { get; set; }
    public DbSet<HousekeepingTask> HousekeepingTasks { get; set; }
    public DbSet<Discount> Discounts { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // User Roles
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Role).HasMaxLength(50).IsRequired(); // Admin, Guest, Housekeeping, FrontDesk
        });

        // Rooms
        builder.Entity<Room>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.RoomNumber).HasMaxLength(10).IsRequired();
            entity.Property(r => r.RoomType).HasMaxLength(50).IsRequired();
            entity.Property(r => r.PricePerNight).IsRequired();
            entity.Property(r => r.Status).HasMaxLength(20).IsRequired(); // Available, Booked, Maintenance
        });

        // Reservations
        builder.Entity<Reservation>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasOne(r => r.User)
                  .WithMany()
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(r => r.Room)
                  .WithMany()
                  .HasForeignKey(r => r.RoomId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(r => r.CheckInDate).IsRequired();
            entity.Property(r => r.CheckOutDate).IsRequired();
            entity.Property(r => r.Status).HasMaxLength(20).IsRequired(); // Pending, Confirmed, CheckedIn, Completed
        });

        // Payments
        builder.Entity<Payment>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasOne(p => p.Reservation)
                  .WithOne()
                  .HasForeignKey<Payment>(p => p.ReservationId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(p => p.Amount).IsRequired();
            entity.Property(p => p.PaymentDate).IsRequired();
            entity.Property(p => p.PaymentMethod).HasMaxLength(50).IsRequired(); // Card, PayPal, Bank Transfer
            entity.Property(p => p.PaymentStatus).HasMaxLength(20).IsRequired(); // Pending, Completed, Failed
        });

        // Reviews
        builder.Entity<Review>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasOne(r => r.User)
                  .WithMany()
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(r => r.Room)
                  .WithMany()
                  .HasForeignKey(r => r.RoomId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(r => r.Rating).IsRequired();
            entity.Property(r => r.Comment).HasMaxLength(500);
            entity.Property(r => r.ReviewDate).IsRequired();
        });

        // Housekeeping Tasks
        builder.Entity<HousekeepingTask>(entity =>
        {
            entity.HasKey(h => h.Id);
            entity.HasOne(h => h.Room)
                  .WithMany()
                  .HasForeignKey(h => h.RoomId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(h => h.Status).HasMaxLength(20).IsRequired(); // Pending, Completed
        });

        // Service Requests
        builder.Entity<ServiceRequest>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.HasOne(s => s.User)
                  .WithMany()
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(s => s.RequestType).HasMaxLength(100).IsRequired(); // Spa, Airport Transfer, Room ServiceA
            entity.Property(s => s.Status).HasMaxLength(20).IsRequired(); // Pending, Completed
        });

        // Discounts & Promotions
        builder.Entity<Discount>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Code).HasMaxLength(20).IsRequired();
            entity.Property(d => d.DiscountPercentage).IsRequired();
            entity.Property(d => d.ExpirationDate).IsRequired();
        });

        // Notifications
        builder.Entity<Notification>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.HasOne(n => n.User)
                  .WithMany()
                  .HasForeignKey(n => n.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(n => n.Message).HasMaxLength(500).IsRequired();
            entity.Property(n => n.IsRead).IsRequired();
        });
    }
}
