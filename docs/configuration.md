# Configuration Reference

This page summarizes option classes available in Markop Test.

## UnitTestOptions

Namespace: `MarkopTest.UnitTest`

- `HostSeparation` (`bool`, default: `false`)
  - When `true`, each test class can use a separate host instance.

## IntegrationTestOptions

Namespace: `MarkopTest.IntegrationTest`

- `LogResponse` (`bool`, default: `true`)
  - Writes JSON responses to test output.
- `HostSeparation` (`bool`, default: `false`)
  - Uses a separated host where needed.

## FunctionalTestOptions

Namespace: `MarkopTest.FunctionalTest`

- `HostSeparation` (`bool`, default: `false`)
  - Uses a separated host where needed.

## LoadTestOptions

Namespace: `MarkopTest.LoadTest`

- `Proxy` (`WebProxy`)
  - Optional proxy used by the `HttpClient`.
- `BaseAddress` (`Uri`)
  - Optional base URL for load tests (useful for external environments).
- `SyncRequestCount` (`int`, default: `50`)
  - Number of sequential requests.
- `AsyncRequestCount` (`int`, default: `1000`)
  - Number of concurrent requests.
- `HostSeparation` (`bool`, default: `false`)
  - Uses a separated host where needed.
- `BaseColor` (`string`, default: `#427bcb`)
  - Primary color used in generated HTML report visualizations.
- `OpenResultAfterFinished` (`bool`, default: `true`)
  - Opens generated report after load test completes.

## Endpoint Attribute

Namespace: `MarkopTest.Attributes`

Use `[Endpoint("...")]` on class or method level. Template supports:

- `[controller]`
- `[action]`

The library resolves these placeholders at runtime based on your test class and method names.

## Test Handler Hooks

Namespace: `MarkopTest.Handler`

Create custom request hooks by inheriting `TestHandler`:

- `BeforeRequest(HttpClient client)`
- `AfterRequest(HttpClient client)`

This is useful for setting headers, authentication cookies, or other request context before and after each request.
