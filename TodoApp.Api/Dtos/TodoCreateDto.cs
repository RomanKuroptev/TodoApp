namespace TodoApp.Api.Dtos;

public sealed record class TodoCreateDto(string Title, DateTime? DueDate);
