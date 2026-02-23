using TodoApi.Models;
using TodoApi.Repositories;

namespace TodoApi.Services;

/// <summary>
/// Service for managing todo items (business logic).
/// </summary>
public class TodoService : ITodoService
{
    private readonly ITodoRepository _repository;

    public TodoService(ITodoRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<TodoItem>> GetAllAsync(TodoStatus? status = null)
    {
        return await _repository.GetAllAsync(status);
    }

    public async Task<TodoItem?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<TodoItem> CreateAsync(CreateTodoRequest request)
    {
        var todoItem = new TodoItem
        {
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            Status = TodoStatus.Todo,
            CreatedAt = DateTime.UtcNow
        };

        return await _repository.AddAsync(todoItem);
    }

    public async Task<TodoItem?> UpdateAsync(int id, UpdateTodoRequest request)
    {
        var todoItem = await _repository.GetByIdAsync(id);

        if (todoItem == null)
        {
            return null;
        }

        todoItem.Title = request.Title;
        todoItem.Description = request.Description;
        todoItem.Status = request.Status;
        todoItem.Priority = request.Priority;
        todoItem.CreatedAt = DateTime.UtcNow;

        return await _repository.UpdateAsync(todoItem);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var todoItem = await _repository.GetByIdAsync(id);

        if (todoItem == null)
        {
            return false;
        }

        await _repository.DeleteAsync(todoItem);
        return true;
    }

    public async Task<IEnumerable<TodoItem>> SearchByTitleAsync(string title)
    {
        return await _repository.SearchByTitleAsync(title);
    }

    public async Task BulkDeleteAsync(string ids)
    {
        var idList = ids.Split(',').Select(int.Parse).ToList();
        
        foreach (var id in idList)
        {
            await DeleteAsync(id);
        }
    }
}

