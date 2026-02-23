using TodoApi.Models;

namespace TodoApi.Services;

/// <summary>
/// Interface for todo service operations.
/// </summary>
public interface ITodoService
{
    Task<IEnumerable<TodoItem>> GetAllAsync(TodoStatus? status = null);
    Task<IEnumerable<TodoItem>> SearchByTitleAsync(string title);
    Task<TodoItem?> GetByIdAsync(int id);
    Task<TodoItem> CreateAsync(CreateTodoRequest request);
    Task<TodoItem?> UpdateAsync(int id, UpdateTodoRequest request);
    Task<bool> DeleteAsync(int id);
    Task BulkDeleteAsync(string ids);
}

