# Integration Tests for Repository Layer

This directory contains both **unit tests** (using mocks) and **integration tests** (using real databases) for the repository layer.

## Test Types

### Unit Tests (Mock-based)
Files ending with `RepositoryShould.cs` contain unit tests that use Moq to mock repository interfaces. These tests:
- Verify the contract and behavior of repository interfaces
- Are fast and don't require database setup
- Always run on all platforms
- Example: `CategoryMandatoryAttributeRepositoryShould.cs`

### Integration Tests (Database-based)
Files ending with `RepositoryIntegrationShould.cs` contain integration tests that use real databases. These tests:
- Validate actual SQL queries and Dapper mappings
- Execute against a real SQL Server LocalDB instance
- Catch regressions in query logic, SQL syntax, and data mapping
- Example: `CategoryMandatoryAttributeRepositoryIntegrationShould.cs`

## Running Integration Tests

### Prerequisites
Integration tests require **SQL Server LocalDB**, which is only available on **Windows**. 

On **non-Windows platforms** (Linux, macOS), integration tests automatically skip with a passing status.

### Database Setup
Integration tests use the connection string: `Server=(localdb)\MSSQLLocalDB;Database=CanoEh;Trusted_Connection=True;`

They expect:
1. SQL Server LocalDB installed and running
2. The `CanoEh` database created with the required schema
3. The necessary tables (e.g., `CategoryMandatoryAttribute`, `ProductNode`)

### Running Tests

**Run all tests** (unit + integration):
```bash
dotnet test Infrastructure.Repositories.Tests/Infrastructure.Repositories.Tests.csproj
```

**Run only integration tests**:
```bash
dotnet test Infrastructure.Repositories.Tests/Infrastructure.Repositories.Tests.csproj --filter "FullyQualifiedName~IntegrationShould"
```

**Run only unit tests** (mocks):
```bash
dotnet test Infrastructure.Repositories.Tests/Infrastructure.Repositories.Tests.csproj --filter "FullyQualifiedName~RepositoryShould&FullyQualifiedName!~IntegrationShould"
```

## Test Cleanup

Integration tests implement `IDisposable` and clean up test data automatically:
- Test ProductNode records are deleted after each test
- CategoryMandatoryAttribute records are deleted (CASCADE constraint handles this)
- Cleanup is skipped on non-Windows platforms

## Adding New Integration Tests

When adding new repository methods that involve SQL/Dapper:

1. **Add unit tests** in `*RepositoryShould.cs` using Moq
2. **Add integration tests** in `*RepositoryIntegrationShould.cs` using real database
3. Follow the existing pattern:
   - Check `IsLocalDbAvailable()` at the start of each test
   - Return early if not on Windows
   - Use test helper methods to create/cleanup test data
   - Use `!` null-forgiving operator when accessing `_repository` after LocalDB check

Example:
```csharp
[Fact]
public async Task MyNewMethod_ShouldWork()
{
    if (!IsLocalDbAvailable())
    {
        return; // Skip on non-Windows
    }
    
    // Arrange
    var testData = await CreateTestDataAsync();
    
    // Act
    var result = await _repository!.MyNewMethodAsync(testData);
    
    // Assert
    Assert.NotNull(result);
}
```

## Benefits of Integration Tests

1. **Catch SQL errors**: Syntax errors, missing columns, wrong table names
2. **Validate Dapper mappings**: Ensure C# properties map correctly to database columns
3. **Test query logic**: Verify ORDER BY, WHERE clauses, and JOIN operations work correctly
4. **Prevent regressions**: Changes to queries are validated against real data
5. **Document expected behavior**: Integration tests serve as living documentation of how queries should work

## Example: CategoryMandatoryAttributeRepository

The `CategoryMandatoryAttributeRepositoryIntegrationShould.cs` file demonstrates integration testing:

- **GetAttributesByCategoryNodeIdAsync**: Verifies ordering by SortOrder works correctly
- **DeleteAttributesByCategoryNodeIdAsync**: Confirms deletion returns correct boolean values
- **Null handling**: Tests that NULL SortOrder values are handled correctly

These tests caught issues that unit tests with mocks would miss, such as:
- Incorrect SQL ORDER BY clause
- Wrong column names in queries
- Dapper mapping configuration issues
