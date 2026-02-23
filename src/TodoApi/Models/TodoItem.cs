namespace TodoApi.Models;

/// <summary>
/// Represents a todo item in the system.
/// </summary>
public class TodoItem
{
    public int Id { get; set; }
    
    public string Title { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public TodoStatus Status { get; set; } = TodoStatus.Todo;
    
    /// <summary>
    /// Priority order in the list. Lower numbers = higher priority (1 = first, 2 = second, etc.)
    /// </summary>
    public int Priority { get; set; } = 1;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

