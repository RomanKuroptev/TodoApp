using System.ComponentModel.DataAnnotations;

namespace TodoApp.Api.Models;

public sealed class Todo
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Title { get; set; }

    public bool IsDone { get; set; }

    public DateTime? DueDate { get; set; }
}
