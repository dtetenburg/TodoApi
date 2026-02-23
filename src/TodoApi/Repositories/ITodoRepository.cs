using TodoApi.Models;

namespace TodoApi.Repositories;

/// <summary>
/// Interface for todo repository operations (data access).
/// </summary>
public interface ITodoRepository
{
    Task<IEnumerable<TodoItem>> GetAllAsync(TodoStatus? status = null);
    Task<IEnumerable<TodoItem>> SearchByTitleAsync(string title);
    Task<TodoItem?> GetByIdAsync(int id);
    Task<TodoItem> AddAsync(TodoItem todoItem);
    Task<TodoItem> UpdateAsync(TodoItem todoItem);
    Task DeleteAsync(TodoItem todoItem);
}

