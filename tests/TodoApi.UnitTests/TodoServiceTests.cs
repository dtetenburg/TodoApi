using FluentAssertions;
using NSubstitute;
using TodoApi.Models;
using TodoApi.Repositories;
using TodoApi.Services;

namespace TodoApi.UnitTests;

/// <summary>
/// Unit tests for the TodoService class.
/// These tests use mocked repository to test service logic in isolation.
/// </summary>
public class TodoServiceTests
{
    private readonly ITodoRepository _mockRepository;
    private readonly TodoService _service;

    public TodoServiceTests()
    {
        _mockRepository = Substitute.For<ITodoRepository>();
        _service = new TodoService(_mockRepository);
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenNoTodosExist()
    {
        // Arrange
        _mockRepository.GetAllAsync(null)
            .Returns(new List<TodoItem>());

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllTodos_WhenTodosExist()
    {
        // Arrange
        var todos = new List<TodoItem>
        {
            new() { Id = 1, Title = "Todo 1" },
            new() { Id = 2, Title = "Todo 2" }
        };
        _mockRepository.GetAllAsync(null)
            .Returns(todos);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_CallsRepositoryWithStatus_WhenStatusProvided()
    {
        // Arrange
        _mockRepository.GetAllAsync(TodoStatus.InProgress)
            .Returns(new List<TodoItem>());

        // Act
        await _service.GetAllAsync(status: TodoStatus.InProgress);

        // Assert
        await _mockRepository.Received(1).GetAllAsync(TodoStatus.InProgress);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ReturnsTodo_WhenTodoExists()
    {
        // Arrange
        var todo = new TodoItem { Id = 1, Title = "Test Todo" };
        _mockRepository.GetByIdAsync(1)
            .Returns(todo);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Test Todo");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenTodoDoesNotExist()
    {
        // Arrange
        _mockRepository.GetByIdAsync(999)
            .Returns((TodoItem?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_CreatesTodo_WithCorrectProperties()
    {
        // Arrange
        var request = new CreateTodoRequest
        {
            Title = "New Todo",
            Description = "Description",
            Priority = 1
        };

        _mockRepository.AddAsync(Arg.Any<TodoItem>())
            .Returns(callInfo =>
            {
                var item = callInfo.Arg<TodoItem>();
                item.Id = 1;
                return item;
            });

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Id.Should().Be(1);
        result.Title.Should().Be("New Todo");
        result.Description.Should().Be("Description");
        result.Priority.Should().Be(1);
        result.Status.Should().Be(TodoStatus.Todo);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateAsync_CallsRepositoryAdd()
    {
        // Arrange
        var request = new CreateTodoRequest { Title = "Test" };
        _mockRepository.AddAsync(Arg.Any<TodoItem>())
            .Returns(callInfo => callInfo.Arg<TodoItem>());

        // Act
        await _service.CreateAsync(request);

        // Assert
        await _mockRepository.Received(1).AddAsync(Arg.Is<TodoItem>(t => t.Title == "Test"));
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_UpdatesTodo_WhenTodoExists()
    {
        // Arrange
        var existingTodo = new TodoItem { Id = 1, Title = "Original" };
        _mockRepository.GetByIdAsync(1)
            .Returns(existingTodo);
        _mockRepository.UpdateAsync(Arg.Any<TodoItem>())
            .Returns(callInfo => callInfo.Arg<TodoItem>());

        var updateRequest = new UpdateTodoRequest
        {
            Title = "Updated",
            Description = "New Description",
            Status = TodoStatus.Done,
            Priority = 5
        };

        // Act
        var result = await _service.UpdateAsync(1, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Updated");
        result.Description.Should().Be("New Description");
        result.Status.Should().Be(TodoStatus.Done);
        result.Priority.Should().Be(5);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenTodoDoesNotExist()
    {
        // Arrange
        _mockRepository.GetByIdAsync(999)
            .Returns((TodoItem?)null);

        var updateRequest = new UpdateTodoRequest { Title = "Updated" };

        // Act
        var result = await _service.UpdateAsync(999, updateRequest);

        // Assert
        result.Should().BeNull();
        await _mockRepository.DidNotReceive().UpdateAsync(Arg.Any<TodoItem>());
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenTodoExists()
    {
        // Arrange
        var todo = new TodoItem { Id = 1, Title = "To Delete" };
        _mockRepository.GetByIdAsync(1)
            .Returns(todo);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_CallsRepositoryDelete_WhenTodoExists()
    {
        // Arrange
        var todo = new TodoItem { Id = 1, Title = "To Delete" };
        _mockRepository.GetByIdAsync(1)
            .Returns(todo);

        // Act
        await _service.DeleteAsync(1);

        // Assert
        await _mockRepository.Received(1).DeleteAsync(todo);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenTodoDoesNotExist()
    {
        // Arrange
        _mockRepository.GetByIdAsync(999)
            .Returns((TodoItem?)null);

        // Act
        var result = await _service.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
        await _mockRepository.DidNotReceive().DeleteAsync(Arg.Any<TodoItem>());
    }

    #endregion

    #region ========== ASSIGNMENT: Implement these tests ==========

    [Fact]
    public void CreateAsync_WhenPriorityIsZero_ShouldRejectOrDefault()
    {
        // Assignment Part 2 - Test 6
        throw new NotImplementedException("ASSIGNMENT: Implement this test - see Assignment.md Part 2, Test 6");
    }

    [Fact]
    public void CreateAsync_WhenPriorityIsNegative_ShouldThrowException()
    {
        // Assignment Part 2 - Test 7
        throw new NotImplementedException("ASSIGNMENT: Implement this test - see Assignment.md Part 2, Test 7");
    }

    [Fact]
    public void UpdateAsync_ShouldNotChangeCreatedAt()
    {
        // Assignment Part 2 - Test 8
        throw new NotImplementedException("ASSIGNMENT: Implement this test - see Assignment.md Part 2, Test 8");
    }

    #endregion

    #region ========== BONUS: Security Tests ==========

    [Fact]
    public void BulkDeleteAsync_WhenIdsContainInvalidValues_ShouldHandleGracefully()
    {
        // BONUS Assignment - Bulk Delete Input Validation
        throw new NotImplementedException("BONUS: Implement this test - see Assignment.md Bonus Test 2");
    }

    [Fact]
    public void BulkDeleteAsync_WhenIdsIsEmpty_ShouldHandleGracefully()
    {
        // BONUS Assignment - Empty Input
        throw new NotImplementedException("BONUS: Implement this test - see Assignment.md Bonus Test 2");
    }

    [Fact]
    public void SearchByTitleAsync_WhenTitleIsNull_ShouldHandleGracefully()
    {
        // BONUS Assignment - Null Search
        throw new NotImplementedException("BONUS: Implement this test - see Assignment.md Bonus Test 3");
    }

    #endregion
}
