using System.ComponentModel.DataAnnotations;

namespace TodoApi.Models;

/// <summary>
/// DTO for updating an existing todo item.
/// </summary>
public class UpdateTodoRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    public TodoStatus Status { get; set; }
    
    /// <summary>
    /// Priority order in the list. Lower numbers = higher priority (1 = first, 2 = second, etc.)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Priority must be at least 1")]
    public int Priority { get; set; } = 1;
}

