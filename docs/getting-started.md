# Getting Started

This guide helps you set up Markop Test in a new or existing solution.

## Prerequisites

- .NET SDK 8.0 or newer
- An ASP.NET application project to test
- xUnit test infrastructure

## 1) Create a Test Project

```bash
dotnet new classlib -n IntegrationTest
dotnet sln add IntegrationTest
```

You can also start with `dotnet new xunit -n IntegrationTest` if you prefer an xUnit template.

## 2) Reference Your Application Project

From the test project directory:

```bash
dotnet add reference ../src/WebAPI/WebAPI.csproj
```

Use the correct project path for your solution.

## 3) Install Markop Test

```bash
dotnet add package MarkopTest
```

## 4) Create an App Factory

Choose a factory based on the test type. Example for integration tests:

```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;
using MarkopTest.IntegrationTest;
using Microsoft.Extensions.DependencyInjection;
using WebAPI;
using Xunit.Abstractions;

public class AppFactory : IntegrationTestFactory<Startup, dynamic>
{
    public AppFactory(ITestOutputHelper outputHelper, HttpClient defaultClient = null)
        : base(outputHelper, defaultClient)
    {
    }

    protected override string GetUrl(string url, string controllerName, string testMethodName)
    {
        return "/api/v1/" + url;
    }

    protected override Task Initializer(IServiceProvider hostServices)
    {
        return Task.CompletedTask;
    }

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        // Replace services for test environment (DB, cache, auth, etc.)
    }

    protected override Task<bool> ValidateResponse(HttpResponseMessage response, dynamic options)
    {
        return Task.FromResult(response.IsSuccessStatusCode);
    }
}
```

## 5) Write a Test Class

```csharp
using MarkopTest.Attributes;
using Xunit;
using Xunit.Abstractions;

[Endpoint("[controller]/[action]")]
public class AccountTests : AppFactory
{
    public AccountTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [MarkopTest.Attributes.Theory]
    [InlineData("user@example.com", "password")]
    public void SignIn(string login, string password)
    {
        PostJson(new { Login = login, Password = password });
    }
}
```

## 6) Run Tests

```bash
dotnet test
```

Or run a specific project:

```bash
dotnet test sample/test/IntegrationTest/IntegrationTest.csproj
```
