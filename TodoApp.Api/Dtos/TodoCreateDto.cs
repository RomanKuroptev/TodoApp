namespace TodoApp.Api.Dtos;

/// <summary>
/// Data transfer object for creating a new todo item.
/// </summary>
/// <param name="Title">The title/description of the todo item (required).</param>
/// <param name="DueDate">Optional due date for the todo item.</param>
public sealed record class TodoCreateDto(string Title, DateTime? DueDate);
