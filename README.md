# Markop Test

[![NuGet](http://img.shields.io/nuget/v/MarkopTest.svg?label=NuGet)](https://www.nuget.org/packages/MarkopTest/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MarkopTest.svg)](https://nuget.org/packages/MarkopTest)
[![MIT License](https://img.shields.io/github/license/MarkopDev/MarkopTest.svg?color=yellow)](https://github.com/MarkopDev/MarkopTest/LICENSE.txt)
[![GitHub stars](https://img.shields.io/github/stars/MarkopDev/MarkopTest.svg?color=red)](https://github.com/MarkopDev/MarkopTest/stargazers)

Markop Test is an open-source testing toolkit for ASP.NET applications. It provides reusable factories to build **unit**, **integration**, **functional**, and **load** tests on top of xUnit with minimal boilerplate.

## Why Markop Test

- Reduces setup code for web-hosted tests
- Supports endpoint-oriented integration testing
- Enables scenario-style functional tests
- Generates HTML reports for load testing
- Integrates with standard xUnit workflows

## Supported Frameworks

Markop Test currently targets:

- `.NET 8`
- `.NET 9`
- `.NET 10`

## Installation

Install from NuGet in your test project:

```powershell
Install-Package MarkopTest
```

Or with the .NET CLI:

```bash
dotnet add package MarkopTest
```

## Quick Start

1. Create a test project (class library or xUnit project).
2. Add a reference to the application project you want to test.
3. Install `MarkopTest`.
4. Create an `AppFactory` class that inherits one of the test factories.
5. Write tests by inheriting from your `AppFactory`.

Example project creation:

```bash
dotnet new classlib -n IntegrationTest
dotnet sln add IntegrationTest
dotnet add IntegrationTest package MarkopTest
```

## Core Concepts

### Factory-Based Setup

Each test style has a factory base class:

- `UnitTestFactory<TStartup>`
- `IntegrationTestFactory<TStartup, TFetchOptions>`
- `FunctionalTestFactory<TStartup>`
- `LoadTestFactory<TStartup>`

You typically override these members:

- `Initializer(IServiceProvider hostServices)` to seed data and prepare test state
- `ConfigureTestServices(IServiceCollection services)` to replace or mock dependencies
- `GetUrl(string url, string controllerName, string testMethodName)` for integration/load routing
- `ValidateResponse(HttpResponseMessage response, TFetchOptions options)` for integration assertions

### Endpoint Routing Attribute

Use `[Endpoint("...")]` on test classes or methods. Markop Test replaces:

- `[controller]` with test class name (suffixes like `Test`, `Tests`, and `Controller` are removed)
- `[action]` with the current test method name

## Test Types

### Unit Tests

Use `UnitTestFactory` when testing isolated application behavior while still having host and DI access.

### Integration Tests

Use `IntegrationTestFactory` to send HTTP requests to your in-memory test server and validate full pipeline behavior.

### Functional Tests

Use `FunctionalTestFactory` for end-to-end business scenarios composed of multiple integration-level actions.

### Load Tests

Use `LoadTestFactory` to execute synchronous and asynchronous request batches and export performance reports to `LoadTestResult/...`.

## Documentation

- [Getting Started](docs/getting-started.md)
- [Test Type Guides](docs/test-types.md)
- [Configuration Reference](docs/configuration.md)

## Sample Project

The `sample/` directory demonstrates real usage:

- `sample/test/UnitTest`
- `sample/test/IntegrationTest`
- `sample/test/FunctionalTest`
- `sample/test/LoadTest`

Run sample tests:

```bash
dotnet test sample/test/UnitTest/UnitTest.csproj
dotnet test sample/test/IntegrationTest/IntegrationTest.csproj
dotnet test sample/test/FunctionalTest/FunctionalTest.csproj
dotnet test sample/test/LoadTest/LoadTest.csproj
```

## Contributing

Contributions are welcome. Please open an issue or pull request describing the problem, proposed solution, and test coverage.

## License

This project is licensed under the [MIT License](LICENSE).