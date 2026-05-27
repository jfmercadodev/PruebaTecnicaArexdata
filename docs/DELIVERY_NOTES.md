# Delivery Notes

## Implemented

- Clean Architecture split into domain, application, infrastructure, and web host.
- Domain invariants with dedicated exceptions and value objects.
- CQRS with `MediatR`, validation, logging, cache, and idempotency.
- SQL Server persistence, migrations, seeding, slow query monitoring, and cache invalidation.
- Full controller API without Minimal APIs.
- Blazor Server UI with list/create/edit flows and shared `ProblemDetails` contract rendering.
- Architecture tests, unit tests, integration tests, and BDD traceability docs.

## Pending

- `F4-23` Validate real app -> SQL Server connectivity inside Docker.
- `F4-24` Validate SQL Server volume persistence after container restart.
- `F5-16` Review git history for atomic commits.

## Why pending

- Current machine now reaches Docker Desktop and compose config is valid, but `docker compose up --build` still fails while pulling `mcr.microsoft.com/mssql/server:2022-latest` due a Docker Desktop local blob/input-output error in its image store.
- Repository now has a baseline commit, but history is not yet granular enough to tell the implementation story step by step.

## Trade-offs accepted

- Blazor pages call application layer through a facade inside same host to keep server-side UI simple and deterministic in tests, while external integration contract still remains the controller API.
- Query cache is process-local with `IMemoryCache`; distributed cache was out of scope for this test.
- Domain event dispatch is in-process post-commit, not full outbox.
