# Todo API

A simple Todo API built with ASP.NET Core and PostgreSQL for learning how to test.

## Project Structure

```
TodoApi/
├── src/
│   └── TodoApi/                    # Main API project
│       ├── Controllers/            # API endpoints
│       ├── Data/                   # Database context
│       ├── Models/                 # Domain models and DTOs
│       ├── Repositories/           # Data access layer
│       ├── Services/               # Business logic layer
│       └── Migrations/             # EF Core migrations
├── tests/
│   ├── TodoApi.UnitTests/          # Unit tests (mocked repository)
│   ├── TodoApi.IntegrationTests/   # Integration tests (Testcontainers + PostgreSQL)
│   └── TodoApi.ApiTests/           # API tests (Testcontainers + PostgreSQL)
├── Dockerfile
├── docker-compose.yml
└── TodoApi.sln
```

## Architecture

The application follows a layered architecture:

```
Controller → Service → Repository → Database
```

- **Controllers**: Handle HTTP requests/responses
- **Services**: Business logic (what to do)
- **Repositories**: Data access (how to get/save data)

## Features

- **CRUD Operations** for Todo items
- **Filter** by Status (Todo, InProgress, Done)
- **Priority ordering** (1 = first, 2 = second, etc.)
- **Automatic sorting** by Priority (lowest number first)

## Todo Model

| Property    | Type       | Description                                    |
|-------------|------------|------------------------------------------------|
| Id          | int        | Unique identifier                              |
| Title       | string     | Todo title (required, max 200)                 |
| Description | string?    | Optional description (max 1000)                |
| Status      | TodoStatus | Todo, InProgress, or Done                      |
| Priority    | int        | Order in list (1 = first, 2 = second, etc.)    |
| CreatedAt   | DateTime   | Creation timestamp (UTC)                       |

## API Endpoints

| Method | Endpoint              | Description                          |
|--------|-----------------------|--------------------------------------|
| GET    | /api/todos            | Get all todos (with optional filters)|
| GET    | /api/todos/{id}       | Get a specific todo                  |
| GET    | /api/todos/search     | Search todos by title                |
| POST   | /api/todos            | Create a new todo                    |
| PUT    | /api/todos/{id}       | Update an existing todo              |
| DELETE | /api/todos/{id}       | Delete a todo                        |
| DELETE | /api/todos/bulk       | Bulk delete todos by IDs             |

### Query Parameters for GET /api/todos

- `status` - Filter by status: `Todo`, `InProgress`, `Done`

### Query Parameters for GET /api/todos/search

- `title` - Search term to find in todo titles

### Query Parameters for DELETE /api/todos/bulk

- `ids` - Comma-separated list of todo IDs to delete (e.g., `1,2,3`)

### Example Requests

```bash
# Get all todos (ordered by priority)
curl http://localhost:5000/api/todos


# Get only completed todos
curl http://localhost:5000/api/todos?status=Done

# Get in-progress todos
curl http://localhost:5000/api/todos?status=InProgress

# Search todos by title
curl http://localhost:5000/api/todos/search?title=Learn

# Create a new todo (priority 1 = do first)
curl -X POST http://localhost:5000/api/todos \
  -H "Content-Type: application/json" \
  -d '{"title": "Learn Testing", "description": "Learn unit and integration testing", "priority": 1}'

# Update a todo (change to priority 2 = do second)
curl -X PUT http://localhost:5000/api/todos/1 \
  -H "Content-Type: application/json" \
  -d '{"title": "Learn Testing", "status": "InProgress", "priority": 2}'

# Delete a todo
curl -X DELETE http://localhost:5000/api/todos/1

# Bulk delete multiple todos
curl -X DELETE "http://localhost:5000/api/todos/bulk?ids=1,2,3"
```

## Getting Started

See [Setup.md](Setup.md) for installation requirements and setup instructions.

## Testing Concepts Explained

### Unit Tests (`TodoApi.UnitTests`)

Unit tests test individual components in **isolation**:

#### TodoServiceTests
- Uses **Moq** to mock the `ITodoRepository`
- Tests the `TodoService` business logic without database
- Verifies correct method calls to repository
- Fast execution (no I/O)

#### TodoRepositoryTests  
- Uses **in-memory database** to test data access
- Tests filtering, sorting, and CRUD operations
- Verifies Entity Framework queries work correctly

**Key packages:**
- `xUnit` - Testing framework
- `FluentAssertions` - Readable assertions
- `Moq` - Mocking framework
- `Microsoft.EntityFrameworkCore.InMemory` - In-memory database

### Integration Tests (`TodoApi.IntegrationTests`)

Integration tests test the **full API stack**:

- Use **real PostgreSQL database** via Testcontainers
- Test actual HTTP requests/responses
- Database runs in Docker container
- Tests the complete flow: Controller → Service → Repository → Database

**Key packages:**
- `xUnit` - Testing framework
- `FluentAssertions` - Readable assertions
- `Microsoft.AspNetCore.Mvc.Testing` - WebApplicationFactory for testing
- `Testcontainers.PostgreSql` - Spin up PostgreSQL in Docker for tests

### API Tests (`TodoApi.ApiTests`)

API tests (acceptance tests) test the **API against a real running environment**:

- Run against a **real running API instance** (not in-memory, not testcontainers)
- Test against acceptance/staging environment or local docker-compose
- Test API contracts (HTTP status codes, response formats, headers)
- Test validation rules (required fields, max lengths, value ranges)
- Test query parameters and filtering
- Test full CRUD workflows
- **As close to production as possible**

**What API tests verify:**
- Correct HTTP status codes (200, 201, 204, 400, 404)
- JSON response format and content types
- Location headers on created resources
- Validation error responses
- Query parameter handling
- End-to-end CRUD operations

**Prerequisites:**
1. Start the API first: `docker-compose up`
2. Or set `API_BASE_URL` environment variable to point to your environment

**Key packages:**
- `xUnit` - Testing framework
- `FluentAssertions` - Readable assertions


