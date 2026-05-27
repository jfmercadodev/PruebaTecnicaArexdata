# ProductCatalog.Specs

Feature files in this project are source of truth for BDD flow. This project now also contains executable acceptance specs aligned to selected `BDD-XXX` scenarios.

## Rules

- Write behavior first in `.feature` files.
- Use scenario ids like `BDD-XXX` for traceability.
- Link each scenario to one or more task ids from `docs/PLAN_PRUEBA_TECNICA.md`.
- Implement domain and application code only after scenario exists.
- When behavior lands, update plan and mark related scenario as automated.

## Near-term workflow

1. Refine feature files.
2. Build domain to satisfy domain scenarios first.
3. Add controller and API scenarios after handlers exist.
4. Add or extend executable acceptance tests whenever a scenario moves from designed to automated.

## Current executable coverage

- Product creation scenarios `BDD-001` to `BDD-004`
- Listing and cache scenarios `BDD-008` to `BDD-011`
- Problem details scenarios `BDD-012` to `BDD-014` and `BDD-019`
- UI behavior scenarios `BDD-021`, `BDD-023`, and `BDD-024`
- Startup and seeding scenarios `BDD-017` and `BDD-018`

## Scenario groups

- `Features/ProductCreation.feature`
- `Features/ProductUpdate.feature`
- `Features/ProductListing.feature`
- `Features/ProblemDetails.feature`
- `Features/StartupAndSeeding.feature`
