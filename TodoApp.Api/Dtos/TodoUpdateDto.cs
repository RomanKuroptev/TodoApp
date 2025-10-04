namespace TodoApp.Api.Dtos;

public sealed record class TodoUpdateDto(string? Title, bool? IsDone, DateTime? DueDate);
