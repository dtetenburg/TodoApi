using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TodoApi.Models;

namespace TodoApi.ApiTests;

/// <summary>
/// API tests for the Todos endpoints.
/// Tests run against a real running API instance (acceptance environment).
/// 
/// Prerequisites:
/// - Start the API using: docker-compose up
/// - Or set API_BASE_URL environment variable to point to your environment
/// </summary>
public class TodosApiTests : IDisposable
{
    private readonly HttpClient _client;

    public TodosApiTests()
    {
        _client = ApiTestConfiguration.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    #region API Contract Tests

    [Fact]
    public async Task GetAll_ReturnsJsonArray()
    {
        // Act
        var response = await _client.GetAsync("/api/todos");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        
        var todos = await response.Content.ReadFromJsonAsync<List<TodoItem>>();
        todos.Should().NotBeNull();
        todos.Should().BeOfType<List<TodoItem>>();
    }

    [Fact]
    public async Task GetById_ReturnsCorrectContentType()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/todos", 
            new CreateTodoRequest { Title = "Content Type Test", Priority = 1 });
        var created = await createResponse.Content.ReadFromJsonAsync<TodoItem>();

        // Act
        var response = await _client.GetAsync($"/api/todos/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task Create_ReturnsLocationHeader()
    {
        // Arrange
        var request = new CreateTodoRequest { Title = "Location Header Test", Priority = 1 };

        // Act
        var response = await _client.PostAsJsonAsync("/api/todos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().ContainEquivalentOf("/api/todos/");
    }

    [Fact]
    public async Task Create_ReturnsCreatedTodoWithId()
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
        var todo = await response.Content.ReadFromJsonAsync<TodoItem>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        todo.Should().NotBeNull();
        todo!.Id.Should().BeGreaterThan(0);
        todo.Title.Should().Be("New Todo");
        todo.Description.Should().Be("Description");
        todo.Priority.Should().Be(1);
        todo.Status.Should().Be(TodoStatus.Todo);
        todo.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    #endregion

    #region HTTP Status Code Tests

    [Fact]
    public async Task GetById_Returns404_WhenNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/todos/999999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_Returns404_WhenNotFound()
    {
        // Arrange
        var request = new UpdateTodoRequest { Title = "Updated", Priority = 1 };

        // Act
        var response = await _client.PutAsJsonAsync("/api/todos/999999", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_Returns404_WhenNotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/todos/999999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_Returns204_WhenSuccessful()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/todos", 
            new CreateTodoRequest { Title = "To Delete", Priority = 1 });
        var created = await createResponse.Content.ReadFromJsonAsync<TodoItem>();

        // Act
        var response = await _client.DeleteAsync($"/api/todos/{created!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task Create_Returns400_WhenTitleIsEmpty()
    {
        // Arrange
        var request = new CreateTodoRequest { Title = "", Priority = 1 };

        // Act
        var response = await _client.PostAsJsonAsync("/api/todos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_Returns400_WhenTitleIsTooLong()
    {
        // Arrange
        var request = new CreateTodoRequest 
        { 
            Title = new string('a', 201), // Max is 200
            Priority = 1 
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/todos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_Returns400_WhenPriorityIsZero()
    {
        // Arrange
        var request = new CreateTodoRequest { Title = "Test", Priority = 0 };

        // Act
        var response = await _client.PostAsJsonAsync("/api/todos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_Returns400_WhenPriorityIsNegative()
    {
        // Arrange
        var request = new CreateTodoRequest { Title = "Test", Priority = -1 };

        // Act
        var response = await _client.PostAsJsonAsync("/api/todos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_Returns400_WhenTitleIsEmpty()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/todos", 
            new CreateTodoRequest { Title = "Original", Priority = 1 });
        var created = await createResponse.Content.ReadFromJsonAsync<TodoItem>();

        var updateRequest = new UpdateTodoRequest { Title = "", Priority = 1 };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/todos/{created!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Query Parameter Tests

    [Fact]
    public async Task GetAll_FiltersbyStatus_Todo()
    {
        // Arrange
        await _client.PostAsJsonAsync("/api/todos", 
            new CreateTodoRequest { Title = "Status Filter Test", Priority = 1 });

        // Act
        var response = await _client.GetAsync("/api/todos?status=Todo");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var todos = await response.Content.ReadFromJsonAsync<List<TodoItem>>();
        todos.Should().OnlyContain(t => t.Status == TodoStatus.Todo);
    }

    [Fact]
    public async Task GetAll_FiltersbyStatus_Done()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/todos", 
            new CreateTodoRequest { Title = "Done Filter Test", Priority = 1 });
        var created = await createResponse.Content.ReadFromJsonAsync<TodoItem>();
        
        await _client.PutAsJsonAsync($"/api/todos/{created!.Id}", 
            new UpdateTodoRequest { Title = "Done Filter Test", Status = TodoStatus.Done, Priority = 1 });

        // Act
        var response = await _client.GetAsync("/api/todos?status=Done");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var todos = await response.Content.ReadFromJsonAsync<List<TodoItem>>();
        todos.Should().OnlyContain(t => t.Status == TodoStatus.Done);
    }

    [Fact]
    public async Task GetAll_ReturnsInPriorityOrder()
    {
        // Arrange
        await _client.PostAsJsonAsync("/api/todos", 
            new CreateTodoRequest { Title = "Priority 3", Priority = 3 });
        await _client.PostAsJsonAsync("/api/todos", 
            new CreateTodoRequest { Title = "Priority 1", Priority = 1 });
        await _client.PostAsJsonAsync("/api/todos", 
            new CreateTodoRequest { Title = "Priority 2", Priority = 2 });

        // Act
        var response = await _client.GetAsync("/api/todos");
        var todos = await response.Content.ReadFromJsonAsync<List<TodoItem>>();

        // Assert
        todos.Should().BeInAscendingOrder(t => t.Priority);
    }

    [Fact]
    public async Task Search_ReturnsOnlyMatchingTodos()
    {
        // Arrange - create todos with different titles
        await _client.PostAsJsonAsync("/api/todos",
            new CreateTodoRequest { Title = "Learn C#", Priority = 1 });
        await _client.PostAsJsonAsync("/api/todos",
            new CreateTodoRequest { Title = "Learn Testing", Priority = 2 });
        await _client.PostAsJsonAsync("/api/todos",
            new CreateTodoRequest { Title = "Buy Groceries", Priority = 3 });

        // Act - search for todos with "Learn" in the title
        var response = await _client.GetAsync("/api/todos/search?title=Learn");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var todos = await response.Content.ReadFromJsonAsync<List<TodoItem>>();
        todos.Should().NotBeNull();
        todos.Should().NotBeEmpty();
        todos.Should().AllSatisfy(t => t.Title.Should().Contain("Learn"));
    }

    #endregion

    #region CRUD Workflow Tests

    [Fact]
    public async Task FullCrudWorkflow_WorksCorrectly()
    {
        // CREATE
        var createRequest = new CreateTodoRequest 
        { 
            Title = "CRUD Test",
            Description = "Testing full workflow",
            Priority = 1
        };
        var createResponse = await _client.PostAsJsonAsync("/api/todos", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<TodoItem>();
        created.Should().NotBeNull();
        var todoId = created!.Id;

        // READ
        var getResponse = await _client.GetAsync($"/api/todos/{todoId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await getResponse.Content.ReadFromJsonAsync<TodoItem>();
        fetched!.Title.Should().Be("CRUD Test");

        // UPDATE
        var updateRequest = new UpdateTodoRequest 
        { 
            Title = "CRUD Test Updated",
            Description = "Updated description",
            Status = TodoStatus.InProgress,
            Priority = 2
        };
        var updateResponse = await _client.PutAsJsonAsync($"/api/todos/{todoId}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResponse.Content.ReadFromJsonAsync<TodoItem>();
        updated!.Title.Should().Be("CRUD Test Updated");
        updated.Status.Should().Be(TodoStatus.InProgress);
        updated.Priority.Should().Be(2);

        // DELETE
        var deleteResponse = await _client.DeleteAsync($"/api/todos/{todoId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // VERIFY DELETED
        var verifyResponse = await _client.GetAsync($"/api/todos/{todoId}");
        verifyResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region ========== BONUS: Security Tests ==========

    [Fact]
    public void Search_ShouldNotBeVulnerableToSqlInjection()
    {
        // BONUS Assignment - SQL Injection
        throw new NotImplementedException("BONUS: Implement this test - see Assignment.md Bonus Test 1");
    }

    #endregion
}

