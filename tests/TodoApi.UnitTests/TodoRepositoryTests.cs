using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Repositories;

namespace TodoApi.UnitTests;

/// <summary>
/// Unit tests for the TodoRepository class.
/// These tests use an in-memory database to test data access logic.
/// </summary>
public class TodoRepositoryTests : IDisposable
{
    private readonly TodoDbContext _context;
    private readonly TodoRepository _repository;

    public TodoRepositoryTests()
    {
        // Create a new in-memory database for each test
        var options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TodoDbContext(options);
        _repository = new TodoRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenNoTodosExist()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllTodos_WhenTodosExist()
    {
        // Arrange
        _context.TodoItems.AddRange(
            new TodoItem { Title = "Todo 1" },
            new TodoItem { Title = "Todo 2" }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByStatus_WhenStatusProvided()
    {
        // Arrange
        _context.TodoItems.AddRange(
            new TodoItem { Title = "Todo", Status = TodoStatus.Todo },
            new TodoItem { Title = "InProgress", Status = TodoStatus.InProgress },
            new TodoItem { Title = "Done", Status = TodoStatus.Done }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync(status: TodoStatus.InProgress);

        // Assert
        result.Should().HaveCount(1);
        result.First().Title.Should().Be("InProgress");
    }

    [Fact]
    public async Task GetAllAsync_OrdersByPriorityAscending()
    {
        // Arrange
        _context.TodoItems.AddRange(
            new TodoItem { Title = "Third", Priority = 3, CreatedAt = DateTime.UtcNow },
            new TodoItem { Title = "First", Priority = 1, CreatedAt = DateTime.UtcNow },
            new TodoItem { Title = "Second", Priority = 2, CreatedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = (await _repository.GetAllAsync()).ToList();

        // Assert
        result[0].Title.Should().Be("First");
        result[1].Title.Should().Be("Second");
        result[2].Title.Should().Be("Third");
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ReturnsTodo_WhenTodoExists()
    {
        // Arrange
        var todo = new TodoItem { Title = "Test Todo" };
        _context.TodoItems.Add(todo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(todo.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Test Todo");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenTodoDoesNotExist()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_AddsTodoToDatabase()
    {
        // Arrange
        var todo = new TodoItem
        {
            Title = "New Todo",
            Description = "Description",
            Priority = 1
        };

        // Act
        var result = await _repository.AddAsync(todo);

        // Assert
        result.Id.Should().BeGreaterThan(0);
        var fromDb = await _context.TodoItems.FindAsync(result.Id);
        fromDb.Should().NotBeNull();
        fromDb!.Title.Should().Be("New Todo");
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_SavesChangesToDatabase()
    {
        // Arrange
        var todo = new TodoItem { Title = "Original" };
        _context.TodoItems.Add(todo);
        await _context.SaveChangesAsync();

        // Act
        todo.Title = "Updated";
        todo.Status = TodoStatus.Done;
        var result = await _repository.UpdateAsync(todo);

        // Assert
        result.Title.Should().Be("Updated");
        var fromDb = await _context.TodoItems.FindAsync(todo.Id);
        fromDb!.Title.Should().Be("Updated");
        fromDb.Status.Should().Be(TodoStatus.Done);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_RemovesTodoFromDatabase()
    {
        // Arrange
        var todo = new TodoItem { Title = "To Delete" };
        _context.TodoItems.Add(todo);
        await _context.SaveChangesAsync();
        var todoId = todo.Id;

        // Act
        await _repository.DeleteAsync(todo);

        // Assert
        var fromDb = await _context.TodoItems.FindAsync(todoId);
        fromDb.Should().BeNull();
    }

    #endregion
}

