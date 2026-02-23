# Setup Guide

This guide walks you through setting up your development environment for the Todo API project.

---

## Requirements

| # | Software | Version | Download |
|---|----------|---------|----------|
| 1 | **Git** | 2.x+ | https://git-scm.com/downloads |
| 2 | **.NET SDK** | **10.0** | https://dotnet.microsoft.com/en-us/download |
| 3 | **Docker Desktop** | Latest | https://www.docker.com/products/docker-desktop |

### Why do I need these?

- **Git** — To clone the repository
- **.NET SDK 10.0** — To build the project and run tests (`dotnet test`)
- **Docker Desktop** — To run the API and PostgreSQL database, and for integration tests that use Testcontainers

> **Note:** Without Docker, only the unit tests will work. Integration tests and API tests require Docker to be running.

---

## Step 1: Verify Installation

Open a terminal and run these commands to verify everything is installed:

```bash
# Check Git
git --version

# Check .NET SDK (should show 10.0.x)
dotnet --version

# Check Docker (Docker Desktop must be running)
docker --version
```

---

## Step 2: Clone the Repository

```bash
git clone <repository-url>
cd TodoApi
```

---

## Step 3: Restore Dependencies

```bash
dotnet restore
```

---

## Step 4: Run the API with Docker

```bash
# Start both the API and PostgreSQL database
docker-compose up --build

# The API will be available at http://localhost:5000
```

To stop:
```bash
docker-compose down
```

To remove all data and start fresh:
```bash
docker-compose down -v
```

---

## Step 5: Run the Tests

### Run All Tests
```bash
dotnet test
```

### Run Only Unit Tests
```bash
dotnet test tests/TodoApi.UnitTests
```

### Run Only Integration Tests
Requires Docker to be running:
```bash
dotnet test tests/TodoApi.IntegrationTests
```

### Run Only API Tests
Requires the API to be running first (`docker-compose up`):
```bash
# Terminal 1: Start the API
docker-compose up

# Terminal 2: Run API tests
dotnet test tests/TodoApi.ApiTests
```

---

## Troubleshooting

### `dotnet: command not found`
The .NET SDK is not installed or not in your PATH. Download it from https://dotnet.microsoft.com/en-us/download and make sure to select **.NET 10.0**.

### `docker: command not found` or Docker connection errors
Make sure Docker Desktop is installed **and running**. On Windows, look for the Docker icon in your system tray.

### Integration tests fail with "Docker is not running"
The integration tests use Testcontainers, which requires Docker. Start Docker Desktop and try again.

### Port 5000 is already in use
Another application is using port 5000. Either stop that application or change the port in `docker-compose.yml`.

### `dotnet restore` fails
Make sure you have .NET SDK **10.0** installed (not just the runtime). Check with `dotnet --list-sdks`.
