using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TodoApp.Api.Data;

namespace TodoApp.Api.Tests;

/// <summary>
/// Custom WebApplicationFactory that sets up a test environment with in-memory SQLite database
/// </summary>
public class TodoAppWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            services.RemoveAll(typeof(DbContextOptions<TodoDbContext>));
            
            // Create and open a connection to an in-memory SQLite database
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            
            // Add DbContext with in-memory SQLite
            services.AddDbContext<TodoDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection?.Close();
            _connection?.Dispose();
        }
        base.Dispose(disposing);
    }
}