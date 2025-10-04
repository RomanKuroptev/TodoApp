using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Data;
using TodoApp.Api.Endpoints;
using TodoApp.Api.Models;

// Make the Program class public for WebApplicationFactory
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Simplified services setup
        builder.Services.AddDbContext<TodoDbContext>(options =>
            options.UseSqlite("Data Source=todo.db"));
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
                policy.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod());
        });

        var app = builder.Build();

        // Setup database and seed data
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
            await db.Database.MigrateAsync();

            if (!await db.Todos.AnyAsync())
            {
                db.Todos.AddRange(
                    new Todo { Title = "Prepare slides", IsDone = false, DueDate = DateTime.UtcNow.AddDays(7) },
                    new Todo { Title = "Book venue", IsDone = false, DueDate = DateTime.UtcNow.AddDays(14) }
                );

                await db.SaveChangesAsync();
            }
        }

        // Enable Swagger for all environments in this demo
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseCors("AllowAll");

        // Map endpoints
        app.MapHealthEndpoints();
        app.MapTodoEndpoints();

        await app.RunAsync();
    }
}
