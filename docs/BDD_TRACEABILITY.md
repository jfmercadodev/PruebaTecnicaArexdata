# BDD Traceability

## Goal

Map behavior scenarios to implementation tasks. Feature files drive sequence. Plan file records completion.

## Scenarios by area

| Scenario ID | Feature | Main tasks | Status |
| --- | --- | --- | --- |
| `BDD-001` | Create valid product | `F1-03`, `F1-04`, `F1-05`, `F1-08`, `F2-03`, `F2-07` | Executable specs + domain/application covered |
| `BDD-002` | Reject sale price below cost | `F1-04`, `F1-05`, `F1-15`, `F2-17` | Executable specs + domain covered |
| `BDD-003` | Reject duplicate sku | `F2-13`, `F2-14`, `F4-07` | Executable specs + controller path covered |
| `BDD-004` | Reuse idempotent response | `F2-05`, `F2-15`, `F3-11` | Executable specs + application covered |
| `BDD-005` | Update price and stock successfully | `F1-05`, `F1-06`, `F1-07`, `F1-08`, `F2-07` | Domain + application covered |
| `BDD-006` | Roll back invalid composite update | `F1-10`, `F2-17` | Domain + application covered |
| `BDD-007` | Reject stock below zero | `F1-07`, `F1-17` | Domain covered |
| `BDD-008` | List products with paging and sorting | `F3-08`, `F3-10`, `F4-07`, `F4-11` | Executable specs + infrastructure/controller/UI covered |
| `BDD-009` | Filter products by name or sku | `F3-10`, `F4-07`, `F4-11` | Executable specs + infrastructure/controller/UI covered |
| `BDD-010` | Show cache source metadata | `F2-08`, `F2-11`, `F2-12`, `F4-07`, `F4-12` | Executable specs + controller payload/UI covered |
| `BDD-011` | Invalidate cache after write | `F2-11`, `F3-13` | Executable specs + application/infrastructure covered |
| `BDD-012` | Return 422 for domain invariant violation | `F4-05`, `F4-07` | Executable specs + web covered |
| `BDD-013` | Return 404 for missing product | `F4-05`, `F4-07` | Executable specs + web covered |
| `BDD-014` | Return 400 for validation error | `F2-13`, `F4-05`, `F4-07` | Executable specs + web covered |
| `BDD-015` | Expose controller endpoints without minimal APIs | `F4-01`, `F4-07` | Web covered |
| `BDD-016` | Start app against SQL Server container | `F0-07`, `F0-08`, `F3-17`, `F3-18`, `F3-19`, `F4-06` | Docker + migration path implemented, smoke pending |
| `BDD-017` | Seed catalog when database is empty | `F3-16`, `F5-13` | Executable specs + infrastructure covered |
| `BDD-018` | Avoid duplicate seeding on restart | `F3-16`, `F5-14` | Executable specs + infrastructure covered |
| `BDD-019` | Propagate correlation id through response and errors | `F4-03`, `F4-04`, `F4-05` | Executable specs + web covered |
| `BDD-020` | Render catalog page with list controls and metadata | `F4-08`, `F4-11`, `F4-12`, `F4-13` | UI render smoke covered |
| `BDD-021` | Block invalid create form until SKU validation passes | `F4-09`, `F4-14`, `F4-17`, `F4-18`, `F4-19` | Executable component specs + UI covered |
| `BDD-022` | Render edit form with original snapshot | `F4-10`, `F4-15`, `F4-17` | UI render smoke covered |
| `BDD-023` | Show technical detail in Development error boundary | `F4-20`, `F4-21` | Executable component specs + UI covered |
| `BDD-024` | Show friendly message in Production error boundary | `F4-20`, `F4-21` | Executable component specs + UI covered |
| `BDD-025` | Read API problem title in UI and expose detail only in Development | `F4-22` | Shared ProblemDetails contract covered by service + integration tests |
