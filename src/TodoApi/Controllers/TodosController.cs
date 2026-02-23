using Microsoft.AspNetCore.Mvc;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodosController : ControllerBase
{
    private readonly ITodoService _todoService;

    public TodosController(ITodoService todoService)
    {
        _todoService = todoService;
    }

    /// <summary>
    /// Get all todo items. Optionally filter by status. Results are ordered by priority (1 = first).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItem>>> GetAll(
        [FromQuery] TodoStatus? status = null)
    {
        var todos = await _todoService.GetAllAsync(status);
        return Ok(todos);
    }

    /// <summary>
    /// Get a specific todo item by id.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TodoItem>> GetById(int id)
    {
        var todo = await _todoService.GetByIdAsync(id);

        if (todo == null)
        {
            return NotFound();
        }

        return Ok(todo);
    }

    /// <summary>
    /// Create a new todo item.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TodoItem>> Create([FromBody] CreateTodoRequest request)
    {
        var todo = await _todoService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = todo.Id }, todo);
    }

    /// <summary>
    /// Search todos by title.
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<TodoItem>>> Search([FromQuery] string? query)
    {
        var todos = await _todoService.SearchByTitleAsync(query);
        return Ok(todos);
    }

    /// <summary>
    /// Bulk delete todos by IDs (comma-separated).
    /// Example: DELETE /api/todos/bulk?ids=1,2,3
    /// </summary>
    [HttpDelete("bulk")]
    public async Task<IActionResult> BulkDelete([FromQuery] string ids)
    {
        await _todoService.BulkDeleteAsync(ids);
        return NoContent();
    }

    /// <summary>
    /// Update an existing todo item.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<TodoItem>> Update(int id, [FromBody] UpdateTodoRequest request)
    {
        var todo = await _todoService.UpdateAsync(id, request);

        if (todo == null)
        {
            return NotFound();
        }

        return Created($"/api/todos/{todo.Id}", todo);
    }

    /// <summary>
    /// Delete a todo item.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _todoService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return Ok();
    }
}

