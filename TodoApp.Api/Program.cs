using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Data;
using TodoApp.Api.Dtos;
using TodoApp.Api.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseSqlite("Data Source=todo.db"));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocal", policy =>
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

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

var swaggerEnabled = app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool?>("Swagger:Enabled") == true;

if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowLocal");
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

var todos = app.MapGroup("/api/todos");

todos.MapGet("/", async (bool? isDone, TodoDbContext db) =>
{
    var query = db.Todos.AsNoTracking();
    if (isDone.HasValue)
    {
        query = query.Where(todo => todo.IsDone == isDone.Value);
    }

    var items = await query.OrderBy(todo => todo.Id).ToListAsync();
    return Results.Ok(items);
});

todos.MapGet("/{id:int}", async (int id, TodoDbContext db) =>
{
    var todo = await db.Todos.AsNoTracking().FirstOrDefaultAsync(todo => todo.Id == id);
    return todo is null ? Results.NotFound() : Results.Ok(todo);
});

todos.MapPost("/", async (TodoCreateDto dto, TodoDbContext db) =>
{
    var errors = ValidationHelpers.ValidateCreate(dto);
    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    var todo = new Todo
    {
        Title = dto.Title.Trim(),
        DueDate = dto.DueDate,
        IsDone = false
    };

    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    return Results.Created($"/api/todos/{todo.Id}", todo);
});

todos.MapPut("/{id:int}", async (int id, TodoUpdateDto dto, TodoDbContext db) =>
{
    var errors = ValidationHelpers.ValidateUpdate(
        dto,
        requireTitle: true,
        hasTitle: true,
        hasIsDone: dto.IsDone.HasValue,
        hasDueDate: true);
    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    var todo = await db.Todos.FirstOrDefaultAsync(todo => todo.Id == id);
    if (todo is null)
    {
        return Results.NotFound();
    }

    todo.Title = dto.Title!.Trim();
    todo.IsDone = dto.IsDone ?? todo.IsDone;
    todo.DueDate = dto.DueDate;

    await db.SaveChangesAsync();

    return Results.Ok(todo);
});

todos.MapPatch("/{id:int}", async (int id, TodoDbContext db, HttpRequest request) =>
{
    if (request.Body.CanSeek)
    {
        request.Body.Seek(0, SeekOrigin.Begin);
    }

    using var reader = new StreamReader(request.Body);
    var body = await reader.ReadToEndAsync();

    if (string.IsNullOrWhiteSpace(body))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["Body"] = new[] { "Request body is required." } });
    }

    using var document = JsonDocument.Parse(body);
    var json = document.RootElement;
    var dto = json.Deserialize<TodoUpdateDto>(new JsonSerializerOptions(JsonSerializerDefaults.Web));

    if (dto is null)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["Body"] = new[] { "Invalid JSON payload." } });
    }

    var hasTitle = json.TryGetProperty("title", out _);
    var hasIsDone = json.TryGetProperty("isDone", out _);
    var hasDueDate = json.TryGetProperty("dueDate", out _);

    var errors = ValidationHelpers.ValidateUpdate(dto, requireTitle: false, hasTitle, hasIsDone, hasDueDate);
    if (errors.Count > 0)
    {
        return Results.ValidationProblem(errors);
    }

    var todo = await db.Todos.FirstOrDefaultAsync(todo => todo.Id == id);
    if (todo is null)
    {
        return Results.NotFound();
    }

    if (hasTitle && dto.Title is not null)
    {
        todo.Title = dto.Title.Trim();
    }

    if (hasIsDone && dto.IsDone.HasValue)
    {
        todo.IsDone = dto.IsDone.Value;
    }

    if (hasDueDate)
    {
        todo.DueDate = dto.DueDate;
    }

    await db.SaveChangesAsync();

    return Results.Ok(todo);
});

todos.MapDelete("/{id:int}", async (int id, TodoDbContext db) =>
{
    var todo = await db.Todos.FirstOrDefaultAsync(todo => todo.Id == id);
    if (todo is null)
    {
        return Results.NotFound();
    }

    db.Todos.Remove(todo);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

await app.RunAsync();

internal static class ValidationHelpers
{
    public static Dictionary<string, string[]> ValidateCreate(TodoCreateDto dto)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (dto.Title is null || string.IsNullOrWhiteSpace(dto.Title))
        {
            errors["Title"] = new[] { "Title is required." };
        }
        else if (dto.Title.Trim().Length > 200)
        {
            errors["Title"] = new[] { "Title must be 200 characters or fewer." };
        }

        return errors;
    }

    public static Dictionary<string, string[]> ValidateUpdate(
        TodoUpdateDto dto,
        bool requireTitle,
        bool hasTitle = true,
        bool hasIsDone = true,
        bool hasDueDate = true)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (requireTitle && dto.Title is null)
        {
            errors["Title"] = new[] { "Title is required." };
        }

        if (hasTitle)
        {
            if (dto.Title is null)
            {
                errors["Title"] = new[] { "Title cannot be null." };
            }
            else if (string.IsNullOrWhiteSpace(dto.Title))
            {
                errors["Title"] = new[] { "Title cannot be empty." };
            }
            else if (dto.Title.Trim().Length > 200)
            {
                errors["Title"] = new[] { "Title must be 200 characters or fewer." };
            }
        }

        if (hasIsDone && dto.IsDone is null)
        {
            errors["IsDone"] = new[] { "IsDone cannot be null." };
        }

        if (!requireTitle && !hasTitle && !hasIsDone && !hasDueDate)
        {
            errors["Body"] = new[] { "At least one value must be provided." };
        }

        return errors;
    }
}
