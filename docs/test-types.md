# Test Type Guides

Markop Test provides four primary factory abstractions. This page explains when and how to use each one.

## Unit Tests

Base class: `UnitTestFactory<TStartup>`

Use unit tests to verify isolated behavior while still leveraging DI and host-level setup.

### Typical use cases

- Extension methods and utility logic
- Domain services with mocked dependencies
- Data transformation and validation routines

### Required overrides

- `Initializer(IServiceProvider hostServices)`
- `ConfigureTestServices(IServiceCollection services)`

## Integration Tests

Base class: `IntegrationTestFactory<TStartup, TFetchOptions>`

Use integration tests to validate API endpoints, middleware, request/response contracts, and persistence behavior.

### Typical use cases

- API contract validation
- Authentication and authorization flows
- Persistence and transaction behavior

### Required overrides

- `GetUrl(string url, string controllerName, string testMethodName)`
- `Initializer(IServiceProvider hostServices)`
- `ConfigureTestServices(IServiceCollection services)`
- `ValidateResponse(HttpResponseMessage response, TFetchOptions options)`

### Common request helpers

- `Get(query, fetchOptions)`
- `Post(query, content, fetchOptions)`
- `Put(query, content, fetchOptions)`
- `Patch(query, content, fetchOptions)`
- `Delete(query, fetchOptions)`
- `PostJson(body, query, fetchOptions)`
- `PutJson(body, query, fetchOptions)`
- `PatchJson(body, query, fetchOptions)`

## Functional Tests

Base class: `FunctionalTestFactory<TStartup>`

Use functional tests for complete user/business scenarios that combine multiple operations.

### Typical use cases

- Multi-step workflows (create, edit, delete)
- Role-based system behavior across multiple endpoints
- Business acceptance scenarios

### Required overrides

- `Initializer(IServiceProvider hostServices)`
- `ConfigureTestServices(IServiceCollection services)`

### Useful capability

- `GetClient()` lets you construct scenario flows using integration test classes or direct HTTP calls.

## Load Tests

Base class: `LoadTestFactory<TStartup>`

Use load tests to compare endpoint performance under synchronous and asynchronous traffic.

### Typical use cases

- Throughput baseline checks
- Endpoint latency regression checks
- Capacity trend monitoring

### Required overrides

- `Initializer(IServiceProvider hostServices)`
- `ConfigureTestServices(IServiceCollection services)`
- `GetUrl(string url, string controllerName, string testMethodName)`

### Common request helpers

- `Post(content)`
- `Put(content)`
- `Patch(content)`
- `PostJson(data)`
- `PutJson(data)`
- `PatchJson(data)`

### Output

Load test execution writes an HTML-based report to a folder under `LoadTestResult/`.
