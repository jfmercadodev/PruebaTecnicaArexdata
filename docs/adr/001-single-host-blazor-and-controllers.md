# ADR-001: Single Host With Blazor Server And Controllers

- Status: Accepted
- Date: 2026-05-26

## Context

Technical test requires interactive UI plus HTTP contract with full controller behavior and `ProblemDetails`.

## Decision

Use one ASP.NET Core host, `ProductCatalog.Web`, containing:

- `Blazor Server` UI
- MVC `Controllers`
- shared DI graph for application and infrastructure

## Consequences

- Simpler deployment and local runtime.
- Shared logging, correlation id, exception handling, and startup/migration path.
- UI can reuse application services directly when appropriate, while public HTTP contract remains available.

## Alternatives considered

- Separate API + separate Blazor host: cleaner deployment boundaries, but more boilerplate and more runtime moving parts for this exercise.
- Minimal APIs: rejected because requirement explicitly asks for full controllers.
