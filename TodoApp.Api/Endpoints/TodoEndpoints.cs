using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Data;
using TodoApp.Api.Dtos;
using TodoApp.Api.Models;

namespace TodoApp.Api.Endpoints;

public static class TodoEndpoints
{
    public static RouteGroupBuilder MapTodoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/todos")
            .WithTags("Todos");

        group.MapGet("/", GetAllTodos)
            .WithName("GetAllTodos")
            .WithDescription("Get all todos with optional filter by completion status")
            .Produces<List<Todo>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", GetTodoById)
            .WithName("GetTodoById")
            .WithDescription("Get a specific todo by ID")
            .Produces<Todo>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateTodo)
            .WithName("CreateTodo")
            .WithDescription("Create a new todo")
            .Produces<Todo>(StatusCodes.Status201Created);

        group.MapPut("/{id:int}", UpdateTodo)
            .WithName("UpdateTodo")
            .WithDescription("Update an existing todo")
            .Produces<Todo>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", DeleteTodo)
            .WithName("DeleteTodo")
            .WithDescription("Delete a todo")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> GetAllTodos(bool? isDone, TodoDbContext db)
    {
        var query = db.Todos.AsNoTracking();
        if (isDone.HasValue)
        {
            query = query.Where(todo => todo.IsDone == isDone.Value);
        }

        var todos = await query.ToListAsync();
        return Results.Ok(todos);
    }

    private static async Task<IResult> GetTodoById(int id, TodoDbContext db)
    {
        var todo = await db.Todos.FindAsync(id);
        return todo is null ? Results.NotFound() : Results.Ok(todo);
    }

    private static async Task<IResult> CreateTodo(TodoCreateDto dto, TodoDbContext db)
    {
        var todo = new Todo
        {
            Title = dto.Title?.Trim() ?? "New Task",
            DueDate = dto.DueDate,
            IsDone = false
        };

        db.Todos.Add(todo);
        await db.SaveChangesAsync();

        return Results.Created($"/api/todos/{todo.Id}", todo);
    }

    private static async Task<IResult> UpdateTodo(int id, TodoUpdateDto dto, TodoDbContext db)
    {
        var todo = await db.Todos.FindAsync(id);
        if (todo is null)
        {
            return Results.NotFound();
        }

        // Update with provided values
        if (dto.Title != null) todo.Title = dto.Title.Trim();
        if (dto.IsDone.HasValue) todo.IsDone = dto.IsDone.Value;
        todo.DueDate = dto.DueDate;

        await db.SaveChangesAsync();
        return Results.Ok(todo);
    }

    private static async Task<IResult> DeleteTodo(int id, TodoDbContext db)
    {
        var todo = await db.Todos.FindAsync(id);
        if (todo is null)
        {
            return Results.NotFound();
        }

        db.Todos.Remove(todo);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
