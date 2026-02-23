using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TodoApi.Models;

namespace TodoApi.IntegrationTests;

/// <summary>
/// Integration tests for the Todos API endpoints.
/// These tests use a real PostgreSQL database via Testcontainers.
/// </summary>
public class TodosControllerTests : IClassFixture<TodoApiFactory>
{
    private readonly HttpClient _client;

    public TodosControllerTests(TodoApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    #region GET /api/todos

    [Fact]
    public async Task GetAll_ReturnsOk_WithEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/todos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var todos = await response.Content.ReadFromJsonAsync<List<TodoItem>>();
        todos.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAll_ReturnsTodos_AfterCreating()
    {
        // Arrange
        var createRequest = new CreateTodoRequest
        {
            Title = "Integration Test Todo",
            Priority = 1
        };
        await _client.PostAsJsonAsync("/api/todos", createRequest);

        // Act
        var response = await _client.GetAsync("/api/todos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var todos = await response.Content.ReadFromJsonAsync<List<TodoItem>>();
        todos.Should().Contain(t => t.Title == "Integration Test Todo");
    }

    [Fact]
    public async Task GetAll_FiltersbyStatus()
    {
        // Arrange - create todos with different statuses
        await _client.PostAsJsonAsync("/api/todos",
            new CreateTodoRequest { Title = "Filter Todo", Priority = 1 });

        var ipResponse = await _client.PostAsJsonAsync("/api/todos",
            new CreateTodoRequest { Title = "Filter InProgress", Priority = 2 });
        var ipCreated = await ipResponse.Content.ReadFromJsonAsync<TodoItem>();
        await _client.PutAsJsonAsync($"/api/todos/{ipCreated!.Id}",
            new UpdateTodoRequest { Title = "Filter InProgress", Status = TodoStatus.InProgress, Priority = 2 });

        var doneResponse = await _client.PostAsJsonAsync("/api/todos",
            new CreateTodoRequest { Title = "Filter Done", Priority = 3 });
        var doneCreated = await doneResponse.Content.ReadFromJsonAsync<TodoItem>();
        await _client.PutAsJsonAsync($"/api/todos/{doneCreated!.Id}",
            new UpdateTodoRequest { Title = "Filter Done", Status = TodoStatus.Done, Priority = 3 });

        // Act - filter by InProgress only
        var response = await _client.GetAsync("/api/todos?status=InProgress");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var todos = await response.Content.ReadFromJsonAsync<List<TodoItem>>();
        todos.Should().OnlyContain(t => t.Status == TodoStatus.InProgress);
    }

    [Fact]
    public async Task GetAll_OrdersByPriorityAscending()
    {
        // Arrange
        await _client.PostAsJsonAsync("/api/todos", new CreateTodoRequest { Title = "Third", Priority = 3 });
        await _client.PostAsJsonAsync("/api/todos", new CreateTodoRequest { Title = "First", Priority = 1 });
        await _client.PostAsJsonAsync("/api/todos", new CreateTodoRequest { Title = "Second", Priority = 2 });

        // Act
        var response = await _client.GetAsync("/api/todos");
        var todos = await response.Content.ReadFromJsonAsync<List<TodoItem>>();

        // Assert
        var orderedTodos = todos!.Where(t => t.Title is "First" or "Second" or "Third").ToList();
        orderedTodos.Should().BeInAscendingOrder(t => t.Priority);
    }

    #endregion

    #region GET /api/todos/{id}

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenTodoDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync("/api/todos/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_ReturnsTodo_WhenTodoExists()
    {
        // Arrange
        var createRequest = new CreateTodoRequest { Title = "Get By Id Test" };
        var createResponse = await _client.PostAsJsonAsync("/api/todos", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<TodoItem>();

        // Act
        var response = await _client.GetAsync($"/api/todos/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var todo = await response.Content.ReadFromJsonAsync<TodoItem>();
        todo!.Title.Should().Be("Get By Id Test");
    }

    #endregion

    #region POST /api/todos

    [Fact]
    public async Task Create_ReturnsCreated_WithValidRequest()
    {
        // Arrange
        var request = new CreateTodoRequest
        {
            Title = "New Todo",
            Description = "Description",
            Priority = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/todos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var todo = await response.Content.ReadFromJsonAsync<TodoItem>();
        todo!.Title.Should().Be("New Todo");
        todo.Description.Should().Be("Description");
        todo.Priority.Should().Be(1);
        todo.Status.Should().Be(TodoStatus.Todo);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WithEmptyTitle()
    {
        // Arrange
        var request = new CreateTodoRequest { Title = "" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/todos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region PUT /api/todos/{id}

    [Fact]
    public async Task Update_ReturnsOk_WithValidRequest()
    {
        // Arrange
        var createRequest = new CreateTodoRequest { Title = "Original" };
        var createResponse = await _client.PostAsJsonAsync("/api/todos", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<TodoItem>();

        var updateRequest = new UpdateTodoRequest
        {
            Title = "Updated",
            Status = TodoStatus.InProgress,
            Priority = 5
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/todos/{created!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<TodoItem>();
        updated!.Title.Should().Be("Updated");
        updated.Status.Should().Be(TodoStatus.InProgress);
        updated.Priority.Should().Be(5);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenTodoDoesNotExist()
    {
        // Arrange
        var updateRequest = new UpdateTodoRequest { Title = "Updated" };

        // Act
        var response = await _client.PutAsJsonAsync("/api/todos/99999", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DELETE /api/todos/{id}

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenTodoExists()
    {
        // Arrange
        var createRequest = new CreateTodoRequest { Title = "To Delete" };
        var createResponse = await _client.PostAsJsonAsync("/api/todos", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<TodoItem>();

        // Act
        var response = await _client.DeleteAsync($"/api/todos/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenTodoDoesNotExist()
    {
        // Act
        var response = await _client.DeleteAsync("/api/todos/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_RemovesTodo_FromDatabase()
    {
        // Arrange
        var createRequest = new CreateTodoRequest { Title = "To Delete Completely" };
        var createResponse = await _client.PostAsJsonAsync("/api/todos", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<TodoItem>();

        // Act
        await _client.DeleteAsync($"/api/todos/{created!.Id}");
        var getResponse = await _client.GetAsync($"/api/todos/{created.Id}");

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}

