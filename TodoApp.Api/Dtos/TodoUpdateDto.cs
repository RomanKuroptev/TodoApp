namespace TodoApp.Api.Dtos;

/// <summary>
/// Data transfer object for updating an existing todo item.
/// All fields are optional - only provided fields will be updated.
/// </summary>
/// <param name="Title">Updated title/description of the todo item.</param>
/// <param name="IsDone">Updated completion status.</param>
/// <param name="DueDate">Updated due date (use null to clear the due date).</param>
public sealed record class TodoUpdateDto(string? Title, bool? IsDone, DateTime? DueDate);
