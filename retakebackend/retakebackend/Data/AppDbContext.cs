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
    }
}
