# ProductCatalog

Senior .NET technical test implemented with `Clean Architecture`, `CQRS`, `Blazor Server`, full `Controllers`, `SQL Server`, `EF Core`, `Serilog`, in-memory query cache, persisted idempotency, and BDD traceability.

## What is implemented

- Product aggregate with invariants for name, SKU, prices, and stock.
- Value objects `Money` and `Sku`.
- Domain events published only after successful persistence.
- CQRS with `MediatR`, `FluentValidation`, `AutoMapper`, and caching behavior.
- SQL Server persistence with EF Core 8, migrations, seeding, and idempotency table.
- Full controller API under `/api/products`.
- Blazor Server UI for list, create, edit, delete, async SKU validation, and `ProblemDetails` rendering.
- Docker assets for app + SQL Server container startup.
- Unit, integration, architecture, executable BDD specs, and traceability artifacts.
- Executable BDD suite currently validates 17 acceptance scenarios end to end and at component level.

## Solution layout

```text
src/
  ProductCatalog.Domain
  ProductCatalog.Application
  ProductCatalog.Infrastructure
  ProductCatalog.Web
tests/
  ProductCatalog.UnitTests
  ProductCatalog.IntegrationTests
  ProductCatalog.ArchTests
  ProductCatalog.Specs
docs/
  PLAN_PRUEBA_TECNICA.md
  BDD_TRACEABILITY.md
  DELIVERY_NOTES.md
  adr/
```

## Key technical decisions

- Single host `ProductCatalog.Web` serves `Blazor Server` UI and full MVC controllers.
- Business rules live in domain first; handlers orchestrate only.
- Query cache returns metadata: source (`Database` or `Cache`) plus elapsed milliseconds.
- Idempotent command handling persists `RequestId` + serialized response.
- SQL Server is official persistence target. Local orchestration is `docker compose`.

ADR details:

- [ADR-001 Single Host With Blazor Server And Controllers](docs/adr/001-single-host-blazor-and-controllers.md)
- [ADR-002 Publish Domain Events After Commit](docs/adr/002-publish-domain-events-after-commit.md)
- [ADR-003 Use SQL Server In Docker For Local Runtime](docs/adr/003-use-sql-server-in-docker-for-local-runtime.md)

## Run with Docker

Prerequisite: Docker Desktop with `docker compose`.

1. Copy `.env.example` to `.env` if you want custom ports or password.
2. Run:

```powershell
docker compose up --build
```

3. Open:

- UI: `http://localhost:8080/products`
- API: `http://localhost:8080/api/products`

Expected behavior:

- SQL Server container becomes healthy first.
- Web container applies migrations automatically.
- Seeder inserts minimum 20 products when database is empty.

## Run locally without full compose

Use any reachable SQL Server instance and set connection string before `dotnet run`.

PowerShell example:

```powershell
$env:ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=ProductCatalogDb;User Id=sa;Password=SqlServerDev123!;TrustServerCertificate=True;Encrypt=False"
dotnet run --project src/ProductCatalog.Web/ProductCatalog.Web.csproj
```

Alternative on Windows with a local SQL Server instance:

```powershell
$env:ConnectionStrings__DefaultConnection="Server=.\\SQLEXPRESS;Database=ProductCatalogDb;Trusted_Connection=True;TrustServerCertificate=True"
dotnet run --project src/ProductCatalog.Web/ProductCatalog.Web.csproj
```

Default launch profile exposes app on `http://localhost:5173`.

## Useful commands

```powershell
dotnet build ProductCatalog.slnx
dotnet test ProductCatalog.slnx -m:1
dotnet ef migrations list --project src/ProductCatalog.Infrastructure/ProductCatalog.Infrastructure.csproj --startup-project src/ProductCatalog.Web/ProductCatalog.Web.csproj --context ProductCatalog.Infrastructure.Persistence.AppDbContext
```

Manual API samples live in [ProductCatalog.http](ProductCatalog.http).

Validation helpers:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\validate-local.ps1
powershell -ExecutionPolicy Bypass -File .\scripts\validate-docker.ps1
```

`validate-docker.ps1` intenta encontrar `docker.exe` aunque Docker Desktop no este en `PATH`.

## Quality status

Latest verified locally in this workspace:

- `dotnet build ProductCatalog.slnx`
- `dotnet test tests/ProductCatalog.UnitTests/ProductCatalog.UnitTests.csproj --no-build --no-restore`
- `dotnet test tests/ProductCatalog.IntegrationTests/ProductCatalog.IntegrationTests.csproj --no-build --no-restore -m:1`
- `dotnet test tests/ProductCatalog.ArchTests/ProductCatalog.ArchTests.csproj`
- `dotnet test tests/ProductCatalog.Specs/ProductCatalog.Specs.csproj --no-build --no-restore -m:1`
- `dotnet test ProductCatalog.slnx --no-build --no-restore -m:1`

Domain coverage is measured from unit tests with collector and tracked in plan.

## Pending / blocked

Delivery notes and known gaps: [docs/DELIVERY_NOTES.md](docs/DELIVERY_NOTES.md)

Current blockers in this machine:

- Docker Desktop now starts, and `docker compose config` is valid, but `docker compose up --build` currently fails while pulling SQL Server image because Docker Desktop reports a local blob/input-output error in its image store.
- Git history review is still pending; repository now has a few focused commits, but the full implementation story is not yet granular enough.
