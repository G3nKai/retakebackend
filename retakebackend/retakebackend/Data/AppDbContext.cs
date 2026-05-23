using Microsoft.EntityFrameworkCore;
using retakebackend.Models;

namespace retakebackend.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TodoList> TodoLists => Set<TodoList>();
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TodoList>()
            .HasMany(t => t.Items)
            .WithOne(i => i.TodoList)
            .HasForeignKey(i => i.TodoListId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
