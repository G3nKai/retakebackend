using DriverService.Models;
using Microsoft.EntityFrameworkCore;

namespace DriverService.Data;

public class DriverDbContext(DbContextOptions<DriverDbContext> options) : DbContext(options)
{
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<DriverOrderAssignment> DriverOrderAssignments => Set<DriverOrderAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Driver>()
            .HasIndex(d => d.Login)
            .IsUnique();
    }
}
