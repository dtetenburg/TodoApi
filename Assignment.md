# Assignment: Testing the Todo API

In this assignment, you will work with tests for the Todo API. The assignment has two parts:

1. **Part 1:** Fix failing tests by finding and fixing bugs in the production code
2. **Part 2:** Write new tests to find additional hidden bugs

## Learning Goals

- Understand the difference between Unit Tests, Integration Tests, and API Tests
- Learn how to debug failing tests and find bugs in production code
- Practice writing tests that expose bugs
- Practice Test-Driven Development (TDD) mindset

---

# Part 1: Fix the Failing Tests

Several tests are currently failing because there are bugs in the production code. Your job is to:

1. Run the tests
2. Analyze why they fail
3. Find the bug in the production code
4. Fix the bug
5. Verify the test passes

## How to Run Tests

See [Setup.md](Setup.md) for installation requirements and how to run the tests.

```bash
# Run all tests
dotnet test

# Run only unit tests
dotnet test tests/TodoApi.UnitTests

# Run only integration tests (requires Docker)
dotnet test tests/TodoApi.IntegrationTests

# Run only API tests (requires API to be running)
docker-compose up
dotnet test tests/TodoApi.ApiTests
```

---

## Failing Test 1: Repository - Status Filter

**Test file:** `tests/TodoApi.UnitTests/TodoRepositoryTests.cs`

**Test name:** `GetAllAsync_FiltersByStatus_WhenStatusProvided`

**What the test does:**
- Creates 3 todos with different statuses (Todo, InProgress, Done)
- Filters by `InProgress` status
- Expects only 1 result (the InProgress todo)

**The test fails because:** The filter returns more items than expected.

**Your task:** Find the bug in the production code and fix it.

---

## Failing Test 2: Integration - Update Status Code

**Test file:** `tests/TodoApi.IntegrationTests/TodosControllerTests.cs`

**Test name:** `Update_ReturnsOk_WithValidRequest`

**What the test does:**
- Creates a todo
- Updates the todo with a PUT request
- Expects HTTP status code `200 OK`

**The test fails because:** The API returns a different status code than expected.

**Your task:** Find the bug in the production code and fix it.

---

## Failing Test 3: Integration - Delete Status Code

**Test file:** `tests/TodoApi.IntegrationTests/TodosControllerTests.cs`

**Test name:** `Delete_ReturnsNoContent_WhenTodoExists`

**What the test does:**
- Creates a todo
- Deletes the todo with a DELETE request
- Expects HTTP status code `204 No Content`

**The test fails because:** The API returns a different status code than expected.

**Your task:** Find the bug in the production code and fix it.

---

## Failing Test 4: Integration - Status Filter

**Test file:** `tests/TodoApi.IntegrationTests/TodosControllerTests.cs`

**Test name:** `GetAll_FiltersbyStatus`

**What the test does:**
- Creates 3 todos with different statuses: `Todo`, `InProgress`, and `Done`
- Filters by `InProgress` status
- Expects only todos with status `InProgress`

**The test fails because:** The filter returns more items than expected (items with a different status are included).

**Your task:** This is the same bug as Failing Test 1. If you fixed that one, this should pass too.

---

## Failing Test 5: Search Returns Wrong Results

**Test file:** `tests/TodoApi.ApiTests/TodosApiTests.cs`

**Test name:** `Search_ReturnsOnlyMatchingTodos`

**What the test does:**
- Creates 3 todos: "Learn C#", "Learn Testing", and "Buy Groceries"
- Searches with `GET /api/todos/search?title=Learn`
- Expects only the 2 todos that contain "Learn" in the title

**The test fails because:** The search returns all todos instead of only matching ones.

**Your task:** Find why the search endpoint ignores the `title` query parameter and fix it.

**Note:** This test only fails as an **API test** (when running against the real API with `docker-compose up`). There are no unit or integration tests for the search functionality, so those test suites remain green.

---

# Part 2: Write Tests and Fix the Hidden Bugs

After fixing the tests in Part 1, there are 3 more placeholder tests that you need to **implement yourself**. Each placeholder contains a `throw new NotImplementedException(...)` that you must replace with actual test code. Your job is to:

1. **Write the test** — implement the test body so it verifies the expected behavior
2. **Run the test** — observe that it fails because of a hidden bug in the production code
3. **Find and fix the bug** — locate the bug in the production code and fix it so the test passes

The placeholder tests are located in:

**File:** `tests/TodoApi.UnitTests/TodoServiceTests.cs`

Look for the region: `#region ========== ASSIGNMENT: Implement these tests ==========`

## Test 6: Priority Zero

**Test name:** `CreateAsync_WhenPriorityIsZero_ShouldRejectOrDefault`

**What the test should verify:**
- When creating a todo with `Priority = 0`, it should be rejected or default to 1
- Priority 0 doesn't make sense in our system (1 = first, 2 = second, etc.)

**Your task:**
1. **Write the test** — replace the `NotImplementedException` with test code that creates a todo with `Priority = 0` and asserts that the priority is greater than 0
2. **Run the test** — it will fail because the system currently accepts priority 0
3. **Fix the bug** — find and fix the validation in the production code so the test passes

---

## Test 7: Negative Priority

**Test name:** `CreateAsync_WhenPriorityIsNegative_ShouldThrowException`

**What the test should verify:**
- When creating a todo with a negative priority (e.g., -5), it should throw an `ArgumentException`
- Negative priorities don't make sense for ordering

**Your task:**
1. **Write the test** — replace the `NotImplementedException` with test code that tries to create a todo with a negative priority and asserts that an `ArgumentException` is thrown
2. **Run the test** — it will fail because the system currently accepts negative priorities
3. **Fix the bug** — add proper validation in the production code so the test passes

---

## Test 8: CreatedAt Should Not Change

**Test name:** `UpdateAsync_ShouldNotChangeCreatedAt`

**What the test should verify:**
- When updating a todo, the `CreatedAt` timestamp should remain unchanged
- The creation date should never change after the todo is created

**Your task:**
1. **Write the test** — replace the `NotImplementedException` with test code that creates a todo with a known `CreatedAt` value, updates the todo, and asserts that `CreatedAt` has not changed
2. **Run the test** — it will fail because something in the update logic is changing the `CreatedAt` value
3. **Fix the bug** — find and fix the update logic in the production code so the test passes

---

## Tips

1. **Read the error messages carefully** - they tell you what's expected vs what was received
2. **Use the debugger** - set breakpoints to understand what's happening
3. **Start with Part 1** - fix the failing tests first
4. **One bug may cause multiple test failures** - fixing one bug may fix multiple tests
5. **HTTP Status Codes reference:**
   - `200 OK` - Success (with response body)
   - `201 Created` - Resource created (for POST)
   - `204 No Content` - Success (no response body, for DELETE)
   - `400 Bad Request` - Invalid input
   - `404 Not Found` - Resource not found

---

## Grading Criteria

| Criteria | Points |
|----------|--------|
| Part 1: Failing tests 1-4 fixed | 40 |
| Part 1: Failing test 5 (Search) fixed | 10 |
| Part 2: Test 6 (Priority Zero) fixed | 15 |
| Part 2: Test 7 (Negative Priority) fixed | 15 |
| Part 2: Test 8 (CreatedAt) fixed | 20 |
| **Total** | **100** |

---

# ⭐ Bonus Assignment: Security Testing (Advanced)

This bonus assignment is for students who want an extra challenge. The Todo API has two new endpoints with **security vulnerabilities**. Your task is to:

1. Find the security issues
2. Write tests that expose the vulnerabilities
3. Fix the vulnerabilities

## New Endpoints

Two new endpoints have been added to the API:

### 1. Search Endpoint
```
GET /api/todos/search?title={searchTerm}
```
Searches for todos by title.

### 2. Bulk Delete Endpoint
```
DELETE /api/todos/bulk?ids=1,2,3
```
Deletes multiple todos at once by their IDs.

---

## Bonus Test 1: SQL Injection (API Test)

**Difficulty:** ⭐⭐⭐ Hard

The search endpoint may be vulnerable to **SQL Injection**. This is a critical security flaw where an attacker can manipulate database queries by injecting malicious SQL code.

**Your task:**
1. Write an API test that attempts SQL injection on the search endpoint
2. Try search terms like: `'; DROP TABLE "TodoItems"; --` or `' OR '1'='1`
3. Verify if the application properly handles or rejects malicious input
4. Fix the vulnerability in the repository

**Add your test to:** `tests/TodoApi.ApiTests/TodosApiTests.cs`

```csharp
[Fact]
public async Task Search_ShouldNotBeVulnerableToSqlInjection()
{
    // BONUS: Implement this test
    throw new NotImplementedException("BONUS: Implement SQL injection test");
}
```

**Hint:** Look at how the SQL query is constructed in the repository. Is user input properly sanitized?

---

## Bonus Test 2: Input Validation - Bulk Delete (Unit Test)

**Difficulty:** ⭐⭐ Medium

The bulk delete endpoint accepts a comma-separated string of IDs. What happens when:
- The input is empty?
- The input contains non-numeric values like `1,2,abc,3`?
- The input contains negative numbers like `1,-5,3`?

**Your task:**
1. Write unit tests for the `BulkDeleteAsync` method
2. Test with invalid inputs and verify proper error handling
3. Fix the vulnerability by adding input validation

**Add your test to:** `tests/TodoApi.UnitTests/TodoServiceTests.cs`

```csharp
[Fact]
public void BulkDeleteAsync_WhenIdsContainInvalidValues_ShouldHandleGracefully()
{
    // BONUS: Implement this test
    throw new NotImplementedException("BONUS: Implement input validation test");
}

[Fact]
public void BulkDeleteAsync_WhenIdsIsEmpty_ShouldHandleGracefully()
{
    // BONUS: Implement this test
    throw new NotImplementedException("BONUS: Implement empty input test");
}
```

---

## Bonus Test 3: Search Input Validation (Unit Test)

**Difficulty:** ⭐⭐ Medium

What happens when the search title is:
- `null`?
- An empty string?
- An extremely long string (e.g., 10,000 characters)?

**Your task:**
1. Write unit tests for edge cases in search
2. Verify proper handling of null/empty/long inputs
3. Add appropriate validation

**Add your test to:** `tests/TodoApi.UnitTests/TodoServiceTests.cs`

```csharp
[Fact]
public void SearchByTitleAsync_WhenTitleIsNull_ShouldHandleGracefully()
{
    // BONUS: Implement this test
    throw new NotImplementedException("BONUS: Implement null search test");
}
```

---

## Security Testing Tips

1. **Think like an attacker** - What would a malicious user try?
2. **Test boundary conditions** - Empty, null, very long, special characters
3. **Never trust user input** - Always validate and sanitize
4. **Use parameterized queries** - Never concatenate user input into SQL
5. **Check for proper error handling** - Errors should not expose system information

---

## Bonus Grading Criteria

| Criteria | Bonus Points |
|----------|--------------|
| SQL Injection test + fix | +15 |
| Bulk Delete validation test + fix | +10 |
| Search validation test + fix | +10 |
| **Total Bonus** | **+35** |

---

Good luck! 🐛🔍



