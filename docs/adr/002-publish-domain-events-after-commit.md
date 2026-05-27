# ADR-002: Publish Domain Events After Commit

- Status: Accepted
- Date: 2026-05-26

## Context

Product aggregate emits business events such as `ProductCreated` and `ProductUpdated`. Publishing before persistence completes risks side effects for data that never committed.

## Decision

Keep pending domain events inside aggregate and publish them only after successful `SaveChanges` / unit of work completion.

## Consequences

- Event stream matches committed state.
- Integration test can verify no publication occurs when persistence fails.
- Simpler than a full outbox while still protecting consistency inside same process.

## Alternatives considered

- Publish inside aggregate methods: rejected because persistence could still fail afterward.
- Full outbox pattern: stronger for distributed integration, but too large for this exercise scope.
