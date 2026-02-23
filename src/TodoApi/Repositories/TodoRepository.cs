using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Repositories;

/// <summary>
/// Repository for todo data access operations.
/// </summary>
public class TodoRepository : ITodoRepository
{
    private readonly TodoDbContext _context;

    public TodoRepository(TodoDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TodoItem>> GetAllAsync(TodoStatus? status = null)
    {
        var query = _context.TodoItems.AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(t => (int)t.Status >= (int)status.Value);
        }

        return await query.OrderBy(t => t.Priority)
                          .ThenBy(t => t.CreatedAt)
                          .ToListAsync();
    }

    public async Task<IEnumerable<TodoItem>> SearchByTitleAsync(string title)
    {
        var sql = $"SELECT * FROM \"TodoItems\" WHERE \"Title\" LIKE '%{title}%'";
        return await _context.TodoItems.FromSqlRaw(sql).ToListAsync();
    }

    public async Task<TodoItem?> GetByIdAsync(int id)
    {
        return await _context.TodoItems.FindAsync(id);
    }

    public async Task<TodoItem> AddAsync(TodoItem todoItem)
    {
        _context.TodoItems.Add(todoItem);
        await _context.SaveChangesAsync();
        return todoItem;
    }

    public async Task<TodoItem> UpdateAsync(TodoItem todoItem)
    {
        await _context.SaveChangesAsync();
        return todoItem;
    }

    public async Task DeleteAsync(TodoItem todoItem)
    {
        _context.TodoItems.Remove(todoItem);
        await _context.SaveChangesAsync();
    }
}

