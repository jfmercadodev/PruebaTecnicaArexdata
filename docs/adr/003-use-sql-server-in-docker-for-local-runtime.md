# ADR-003: Use SQL Server In Docker For Local Runtime

- Status: Accepted
- Date: 2026-05-26

## Context

Requirement asks for SQL Server and full dockerized solution. Local setup must be reproducible and close to expected runtime shape.

## Decision

Use SQL Server 2022 Developer container in `docker-compose.yml`, plus:

- persistent Docker volume
- healthcheck before web startup
- environment-driven connection string
- automatic migration + seed on application startup

## Consequences

- Consistent local runtime for reviewers.
- Fewer machine-specific differences than local DB installers.
- Delivery depends on Docker availability for final smoke tests.

## Alternatives considered

- SQLite: rejected because requirement changed to SQL Server.
- Local machine SQL Server only: workable, but less reproducible than containerized setup.
