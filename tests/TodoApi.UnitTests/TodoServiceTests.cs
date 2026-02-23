using FluentAssertions;
using Moq;
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
    private readonly Mock<ITodoRepository> _mockRepository;
    private readonly TodoService _service;

    public TodoServiceTests()
    {
        _mockRepository = new Mock<ITodoRepository>();
        _service = new TodoService(_mockRepository.Object);
    }

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenNoTodosExist()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync(null))
            .ReturnsAsync(new List<TodoItem>());

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
        _mockRepository.Setup(r => r.GetAllAsync(null))
            .ReturnsAsync(todos);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_CallsRepositoryWithStatus_WhenStatusProvided()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync(TodoStatus.InProgress))
            .ReturnsAsync(new List<TodoItem>());

        // Act
        await _service.GetAllAsync(status: TodoStatus.InProgress);

        // Assert
        _mockRepository.Verify(r => r.GetAllAsync(TodoStatus.InProgress), Times.Once);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_ReturnsTodo_WhenTodoExists()
    {
        // Arrange
        var todo = new TodoItem { Id = 1, Title = "Test Todo" };
        _mockRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(todo);

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
        _mockRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((TodoItem?)null);

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

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<TodoItem>()))
            .ReturnsAsync((TodoItem item) =>
            {
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
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<TodoItem>()))
            .ReturnsAsync((TodoItem item) => item);

        // Act
        await _service.CreateAsync(request);

        // Assert
        _mockRepository.Verify(r => r.AddAsync(It.Is<TodoItem>(t => t.Title == "Test")), Times.Once);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_UpdatesTodo_WhenTodoExists()
    {
        // Arrange
        var existingTodo = new TodoItem { Id = 1, Title = "Original" };
        _mockRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingTodo);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<TodoItem>()))
            .ReturnsAsync((TodoItem item) => item);

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
        _mockRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((TodoItem?)null);

        var updateRequest = new UpdateTodoRequest { Title = "Updated" };

        // Act
        var result = await _service.UpdateAsync(999, updateRequest);

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<TodoItem>()), Times.Never);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenTodoExists()
    {
        // Arrange
        var todo = new TodoItem { Id = 1, Title = "To Delete" };
        _mockRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(todo);
        _mockRepository.Setup(r => r.DeleteAsync(todo))
            .Returns(Task.CompletedTask);

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
        _mockRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(todo);
        _mockRepository.Setup(r => r.DeleteAsync(todo))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(1);

        // Assert
        _mockRepository.Verify(r => r.DeleteAsync(todo), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenTodoDoesNotExist()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((TodoItem?)null);

        // Act
        var result = await _service.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<TodoItem>()), Times.Never);
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

