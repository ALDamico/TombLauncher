---
description: How to write and run unit tests
---

# Writing Unit Tests

## Framework & Conventions

- **Framework**: xUnit
- **Mocking**: NSubstitute
- **Project**: `tests/TombLauncher.Tests/TombLauncher.Tests.csproj`
- **Global usings**: `Xunit` is globally imported via the `.csproj`

## Creating a Test Class

1. Create a new file in `tests/TombLauncher.Tests/` named `{ClassUnderTest}Tests.cs`
2. Use namespace `TombLauncher.Tests`
3. If the tests create temp files/directories, implement `IDisposable` for cleanup

```csharp
using TombLauncher.Core.SomeNamespace;

namespace TombLauncher.Tests;

public class MyClassTests : IDisposable
{
    private readonly string _tempDir;

    public MyClassTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"MyClassTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public void MethodName_Scenario_ExpectedResult()
    {
        // Arrange
        // Act
        // Assert
    }
}
```

## Naming Convention

- Test method names follow: `MethodName_Scenario_ExpectedResult`
- Example: `GetGamesFolder_ReturnsCorrectSubdirectory`

## Mocking with NSubstitute

```csharp
using NSubstitute;

var mockService = Substitute.For<IMyService>();
mockService.MyMethod(Arg.Any<string>()).Returns("result");
```

## Running Tests

// turbo
```bash
dotnet test tests/TombLauncher.Tests/TombLauncher.Tests.csproj
```
