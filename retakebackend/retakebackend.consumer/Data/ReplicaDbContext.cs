using Microsoft.EntityFrameworkCore;
using retakebackend.consumer.Models;

namespace retakebackend.consumer.Data;

public class ReplicaDbContext(DbContextOptions<ReplicaDbContext> options) : DbContext(options)
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
