using Microsoft.EntityFrameworkCore;
using retakebackend.Models;

namespace retakebackend.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<DriverOrderAssignment> DriverOrderAssignments => Set<DriverOrderAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Driver>()
            .Property(d => d.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Order>()
            .Property(o => o.Status)
            .HasConversion<string>();

        modelBuilder.Entity<DriverOrderAssignment>()
            .HasOne(a => a.Driver)
            .WithMany(d => d.Assignments)
            .HasForeignKey(a => a.DriverId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DriverOrderAssignment>()
            .HasIndex(a => new { a.DriverId, a.OrderId })
            .IsUnique();
    }
}
