using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Models;

namespace TodoApp.Api.Data;

public sealed class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options)
        : base(options)
    {
    }

    public DbSet<Todo> Todos => Set<Todo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Todo>(entity =>
        {
            entity.HasKey(todo => todo.Id);
            entity.Property(todo => todo.Title)
                .IsRequired()
                .HasMaxLength(200);
            entity.Property(todo => todo.IsDone)
                .IsRequired();
        });
    }
}
