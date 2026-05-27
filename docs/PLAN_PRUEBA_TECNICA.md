# Plan de ejecucion - Prueba Tecnica Senior .NET

## Estado actual

- Repositorio Git inicializado.
- Solucion base creada con capas principales y proyectos de test.
- Host web preparado para Blazor Server + Controllers.
- Base BDD creada con features Gherkin y trazabilidad inicial.
- Dominio base implementado y validado con tests unitarios.
- Application base implementada con commands, queries, handlers, contratos e idempotencia.
- Pipeline de application implementado para logging, validacion y cache.
- Mapping centralizado con AutoMapper y tests de contrato activos.
- Infrastructure base implementada con EF Core, SQL Server provider, repos, cache e idempotencia persistida.
- Monitoreo EF Core implementado para slow queries y heuristica N+1.
- Inicializacion de base y seeder idempotente preparados en host.
- Middleware global de excepciones con `ProblemDetails` implementado.
- `ProductsController` completo implementado y validado con tests de integracion web.
- `Serilog` y `CorrelationId` implementados en host web.
- Paginas Blazor de listado, alta y edicion implementadas con validacion y smoke tests.
- `CustomErrorBoundary` implementado para recuperacion visual y mensajes segun entorno.

## Estrategia recomendada

1. Construir primero estructura y dominio. Dominio vale mas puntos y condiciona todo lo demas.
2. Definir comportamiento primero con escenarios BDD y trazar cada escenario contra tareas.
3. Cerrar despues capa de aplicacion con CQRS, validacion, cache y eventos.
4. Integrar infraestructura con EF Core, SQL Server, repositorios, seeder y logging.
5. Dockerizar temprano app + base de datos para no descubrir tarde problemas de runtime.
6. Exponer API interna con Controllers + Blazor Server para cumplir UI, Problem Details y Error Boundary.
7. Cerrar con tests, README, ADRs y smoke tests de entrega.

## Decisiones base sugeridas

- Runtime: `.NET 8`
- Base de datos: `SQL Server` en contenedor Docker
- UI: `Blazor Server`
- Host: un solo proyecto `ProductCatalog.Web` con Blazor Server + `Controllers` completos para `/api/products`
- ORM: `EF Core 8`
- CQRS: `MediatR`
- Validacion: `FluentValidation`
- Logging: `Serilog`
- Cache: `IMemoryCache`
- Mapeo: `AutoMapper` con perfiles centralizados
- Tests: `xUnit` + `NetArchTest` + specs BDD en `tests/ProductCatalog.Specs`
- Orquestacion local: `docker compose`

## Decisiones tecnicas que conviene fijar temprano

- `Money`: usar `readonly record struct` por semantica de valor, menos allocations e inmutabilidad fuerte.
- `Sku`: normalizar en factory y persistir como string via `ValueConverter`.
- Eventos de dominio: guardar eventos pendientes dentro de agregado y publicarlos solo despues de `SaveChanges` exitoso.
- Dedupe de eventos: si producto se crea y luego cambia antes de persistir, conservar solo evento representativo final. Para producto nuevo, solo `ProductCreated`; para producto existente, un solo `ProductUpdated`.
- Cache stampede: implementar bloqueo local por `CacheKey` con `SemaphoreSlim`.
- Idempotencia: almacenar `RequestId` y respuesta serializada en tabla `ProcessedRequests`. Si no alcanza tiempo, dejar diseno documentado.
- Provider EF: `Microsoft.EntityFrameworkCore.SqlServer`.
- Arranque Docker: `docker-compose.yml` con servicios `web` y `sqlserver`, volumen persistente y healthcheck.
- Configuracion: connection string via variables de entorno, nunca fija en codigo.
- Inicializacion DB: aplicar migraciones al iniciar app cuando entorno sea contenedor local.
- BDD: `Feature files -> trazabilidad -> implementacion -> automatizacion`.

## Orden de trabajo sugerido

### Fase 0 - Bootstrap

- [x] `F0-01` Inicializar Git: `git init`
- [x] `F0-02` Crear solucion `ProductCatalog.slnx`
- [x] `F0-03` Crear proyectos:
  - `src/ProductCatalog.Domain`
  - `src/ProductCatalog.Application`
  - `src/ProductCatalog.Infrastructure`
  - `src/ProductCatalog.Web`
  - `tests/ProductCatalog.UnitTests`
  - `tests/ProductCatalog.IntegrationTests`
  - `tests/ProductCatalog.ArchTests`
- `tests/ProductCatalog.Specs`
- [x] `F0-04` Agregar referencias entre proyectos respetando Clean Architecture
- [x] `F0-BDD-01` Crear proyecto `tests/ProductCatalog.Specs` con features Gherkin base
- [x] `F0-BDD-02` Crear trazabilidad escenario -> tarea en `docs/BDD_TRACEABILITY.md`
- [x] `F0-CTRL-01` Preparar host para usar `Controllers` y no Minimal APIs
- [x] `F0-05` Agregar paquetes base
- [x] `F0-06` Crear `.dockerignore`
- [x] `F0-07` Crear `Dockerfile` multi-stage para `ProductCatalog.Web`
- [x] `F0-08` Crear `docker-compose.yml` con `web` + `sqlserver`
- [x] `F0-09` Definir variables de entorno base en `.env.example`
- [x] `F0-10` Confirmar `dotnet build`
- [x] `F0-11` Crear commit base del repositorio

Criterio de salida: solucion compila, dependencias claras, historial empieza bien.

Regla de trabajo: antes de implementar comportamiento nuevo, crear o ajustar escenario BDD correspondiente.

### Fase 1 - Dominio

- [x] `F1-01` Crear base de dominio: `Entity`, `AggregateRoot`, `DomainEvent`, `DomainException`
- [x] `F1-02` Crear excepciones especificas:
  - `InvalidProductNameException`
  - `InvalidSkuException`
  - `InvalidPriceException`
  - `InvalidStockException`
- [x] `F1-03` Implementar `Sku` con normalizacion completa y configuracion `0` vs `O`
- [x] `F1-04` Implementar `Money.Create(decimal value)` con precision y operaciones seguras
- [x] `F1-05` Implementar agregado `Product` con invariantes estrictas
- [x] `F1-06` Implementar `UpdatePrice(decimal precioVenta, decimal costo)`
- [x] `F1-07` Implementar `AdjustStock(int delta)`
- [x] `F1-08` Emitir `ProductCreated` y `ProductUpdated`
- [x] `F1-09` Implementar deduplicacion de eventos pendientes
- [x] `F1-10` Implementar rollback interno tipo snapshot/memento para cambios compuestos
- [x] `F1-11` Implementar `ISpecification<Product>`
- [x] `F1-12` Implementar `ActiveProductSpecification`
- [x] `F1-13` Implementar `LowStockSpecification(int threshold)`
- [x] `F1-14` Implementar composicion `&&`, `||`, `!` y traduccion a `Expression<Func<Product, bool>>`
- [x] `F1-15` Tests unitarios de `Money`
- [x] `F1-16` Tests unitarios de `Sku`
- [x] `F1-17` Tests unitarios de `Product` y eventos

Criterio de salida: dominio aislado, sin EF ni MediatR, cobertura alta y reglas cerradas.

Estado actual de validacion: `dotnet test tests/ProductCatalog.UnitTests/ProductCatalog.UnitTests.csproj --no-build --no-restore` -> 37/37 tests OK.

### Fase 2 - Aplicacion

- [x] `F2-01` Crear DTOs de salida y request models
- [x] `F2-02` Crear `ProductDto` con `MarginPercent`
- [x] `F2-03` Crear commands:
  - `CreateProductCommand`
  - `UpdateProductCommand`
  - `DeleteProductCommand`
- [x] `F2-04` Crear queries:
  - `GetProductsQuery`
  - `GetProductByIdQuery`
- [x] `F2-05` Agregar `RequestId` opcional a commands
- [x] `F2-06` Definir contratos:
  - `IProductReadRepository`
  - `IProductWriteRepository`
  - `IUnitOfWork` o contrato equivalente
  - `IDomainEventPublisher`
  - `IIdempotencyStore`
- [x] `F2-07` Implementar handlers pequenos, solo orquestacion
- [x] `F2-08` Implementar `ICacheableQuery`
- [x] `F2-09` Implementar `LoggingBehavior<TRequest,TResponse>`
- [x] `F2-10` Implementar `ValidationBehavior<TRequest,TResponse>`
- [x] `F2-11` Implementar `CachingBehavior<TRequest,TResponse>`
- [x] `F2-12` Definir respuesta para queries con metadatos de fuente y tiempo:
  - `CacheHit`
  - `ElapsedMs`
- [x] `F2-13` Crear validadores FluentValidation por command
- [x] `F2-14` Validacion asincrona de SKU duplicado via repositorio
- [x] `F2-15` Implementar estrategia de idempotencia
- [x] `F2-16` Configurar perfil de AutoMapper
- [x] `F2-17` Tests unitarios de handlers
- [x] `F2-18` Tests de contrato de mapeo

Criterio de salida: CQRS funcional, validacion y cache activas, handlers limpios.

Estado actual de validacion: `dotnet test tests/ProductCatalog.UnitTests/ProductCatalog.UnitTests.csproj --no-build --no-restore` -> 37/37 tests OK.

### Fase 3 - Infraestructura y SQL Server

- [x] `F3-01` Crear `AppDbContext`
- [x] `F3-02` Configurar entidades con `IEntityTypeConfiguration<T>`
- [x] `F3-03` Configurar indice unico por `SKU`
- [x] `F3-04` Configurar precision `(18,4)` para dinero
- [x] `F3-05` Crear `ValueConverter` para `Money`
- [x] `F3-06` Crear `ValueConverter` para `Sku`
- [x] `F3-07` Implementar query filter para `IsDeleted` si se decide soft-delete
- [x] `F3-08` Implementar repositorio de lectura `AsNoTracking`
- [x] `F3-09` Implementar repositorio de escritura con tracking
- [x] `F3-10` Implementar paginacion, filtros y ordenamiento dinamico sin `switch` exhaustivo
- [x] `F3-11` Implementar persistencia de `ProcessedRequests`
- [x] `F3-12` Implementar publicacion de eventos despues de commit exitoso
- [x] `F3-13` Invalidar cache despues del commit
- [x] `F3-14` Implementar `DbCommandInterceptor` para queries lentas
- [x] `F3-15` Agregar heuristica simple de deteccion N+1 y documentar falsos positivos
- [x] `F3-16` Implementar seeder idempotente con minimo 20 productos
- [x] `F3-17` Configurar `UseSqlServer` y connection string desde configuracion
- [x] `F3-18` Crear migracion inicial para SQL Server
- [x] `F3-19` Asegurar que migraciones se apliquen al iniciar entorno Docker local
- [x] `F3-20` Configurar volumen persistente para datos de SQL Server
- [x] `F3-21` Test de integracion: no publicar evento si `SaveChanges` falla

Criterio de salida: persistencia estable, datos iniciales listos, eventos consistentes.

Estado actual de validacion:
- `dotnet build src/ProductCatalog.Infrastructure/ProductCatalog.Infrastructure.csproj --no-restore` OK
- `dotnet build ProductCatalog.slnx` OK
- `dotnet ef migrations list --project src/ProductCatalog.Infrastructure/ProductCatalog.Infrastructure.csproj --startup-project src/ProductCatalog.Web/ProductCatalog.Web.csproj --context ProductCatalog.Infrastructure.Persistence.AppDbContext --no-build` -> `20260527020801_InitialCreate`
- `dotnet test tests/ProductCatalog.IntegrationTests/ProductCatalog.IntegrationTests.csproj --no-build --no-restore -m:1` -> 25/25 tests OK
- `dotnet test ProductCatalog.slnx --no-build --no-restore -m:1` OK
- `docker compose config` ya es valido y `validate-docker.ps1` llega hasta `compose up`, pero el pull de `mcr.microsoft.com/mssql/server:2022-latest` falla por error local de Docker Desktop sobre blob/input-output en su image store

### Fase 4 - Docker, Web, API y Blazor

- [x] `F4-01` Configurar `ProductCatalog.Web` como host unico con Blazor Server + Controllers
- [x] `F4-02` Registrar DI de Application + Infrastructure
- [x] `F4-03` Configurar Serilog
- [x] `F4-04` Implementar middleware de `CorrelationId`
- [x] `F4-05` Implementar middleware global de excepciones con `ProblemDetails`
- [x] `F4-06` Configurar arranque en contenedor:
  - puerto expuesto app
  - dependencia con healthcheck de SQL Server
  - variables `ConnectionStrings__DefaultConnection`, `SA_PASSWORD`, `ASPNETCORE_ENVIRONMENT`
- [x] `F4-07` Implementar `ProductsController` y endpoints API:
  - `GET /api/products`
  - `GET /api/products/{id}`
  - `POST /api/products`
  - `PUT /api/products/{id}`
  - `DELETE /api/products/{id}`
  - `GET /api/products/sku-exists`
- [x] `F4-08` Crear pagina `/products`
- [x] `F4-09` Crear pagina `/products/new`
- [x] `F4-10` Crear pagina `/products/{id}/edit`
- [x] `F4-11` Crear tabla paginada con filtro por nombre/SKU y ordenamiento
- [x] `F4-12` Mostrar fuente de datos: `Cache` o `Base de datos`
- [x] `F4-13` Mostrar tiempo de respuesta en ms
- [x] `F4-14` Crear formulario de alta con validacion por campo
- [x] `F4-15` Crear formulario de edicion con validacion por campo
- [x] `F4-16` Mostrar errores globales de negocio en parte superior
- [x] `F4-17` Deshabilitar submit si formulario es invalido
- [x] `F4-18` Implementar validacion asincrona de SKU con debounce >= 300 ms
- [x] `F4-19` Cancelar validaciones previas con `CancellationToken`
- [x] `F4-20` Implementar `CustomErrorBoundary`
- [x] `F4-21` En `Development`, mostrar detalle tecnico; en `Production`, mensaje amigable + correlacion
- [x] `F4-22` Si API responde `ProblemDetails`, leer `title` para UI y `detail` solo en `Development`
- [ ] `F4-23` Verificar conectividad real app -> SQL Server dentro de Docker
- [ ] `F4-24` Verificar persistencia tras reinicio de contenedores

Criterio de salida: app navegable, errores consistentes, UX cumple enunciado.

Estado actual de validacion:
- `dotnet build ProductCatalog.slnx` OK
- `dotnet test tests/ProductCatalog.UnitTests/ProductCatalog.UnitTests.csproj --no-build --no-restore` -> 37/37 tests OK
- `dotnet test tests/ProductCatalog.IntegrationTests/ProductCatalog.IntegrationTests.csproj --no-build --no-restore -m:1` -> 25/25 tests OK
- `dotnet test tests/ProductCatalog.Specs/ProductCatalog.Specs.csproj --no-build --no-restore -m:1` -> 17/17 tests OK
- `dotnet test ProductCatalog.slnx --no-build --no-restore -m:1` OK
- Cobertura web validada para `ProblemDetails`, CRUD, `sku-exists`, tabla de rutas, `CorrelationId`, paginas `/products`, `/products/new`, `/products/{id}/edit` y generacion de header
- UI ahora consume contrato compartido de `ProblemDetails`: muestra `title` siempre y `detail` solo en `Development`
- `tests/ProductCatalog.Specs` ahora ejecuta 17 escenarios de aceptacion alineados a `BDD-XXX`, incluyendo validacion asincrona de SKU y `CustomErrorBoundary`

### Fase 5 - Calidad y evidencia

- [x] `F5-01` Agregar tests de arquitectura con `NetArchTest`
- [x] `F5-02` Verificar que `Domain` no referencia `Application` ni `Infrastructure`
- [x] `F5-03` Verificar que handlers no usan `DbContext` directo
- [x] `F5-04` Verificar que Value Objects no tienen setters publicos
- [x] `F5-05` Verificar que eventos de dominio son inmutables
- [x] `F5-06` Ejecutar `dotnet test`
- [x] `F5-07` Medir cobertura de dominio y dejar >= 80%
- [x] `F5-08` Crear `README.md`
- [x] `F5-09` Documentar al menos 3 decisiones arquitectonicas con alternativas descartadas
- [x] `F5-10` Documentar items no implementados y por que
- [x] `F5-11` Crear `docs/adr/` con 3 ADRs minimos
- [x] `F5-12` Crear archivo `.http` para probar API
- [ ] `F5-13` Probar `docker compose up --build` con seeder y UI accesible
- [ ] `F5-14` Probar reinicio completo con volumen persistente de SQL Server
- [x] `F5-15` Dejar documentado flujo alterno local con `dotnet run` usando misma base o connection string equivalente
- [ ] `F5-16` Revisar historial de commits para que sea atomico y descriptivo

Criterio de salida: entrega defendible en revision automatica y en vivo.

Estado actual de validacion:
- `dotnet build ProductCatalog.slnx` OK
- `dotnet test tests/ProductCatalog.ArchTests/ProductCatalog.ArchTests.csproj --no-build --no-restore` -> 4/4 tests OK
- `dotnet test tests/ProductCatalog.UnitTests/ProductCatalog.UnitTests.csproj --no-build --no-restore` -> 37/37 tests OK
- `dotnet test tests/ProductCatalog.IntegrationTests/ProductCatalog.IntegrationTests.csproj --no-build --no-restore -m:1` -> 25/25 tests OK
- `dotnet test tests/ProductCatalog.Specs/ProductCatalog.Specs.csproj --no-build --no-restore -m:1` -> 17/17 tests OK
- `dotnet test ProductCatalog.slnx --no-build --no-restore -m:1` OK
- Cobertura de `ProductCatalog.Domain` medida con `XPlat Code Coverage` -> 80.36%
- `tests/ProductCatalog.Specs` ya actua como suite ejecutable para escenarios automatizados, incluyendo interacciones UI clave, y como repositorio de features para escenarios pendientes
- `git log --oneline` aun requiere mas granularidad para contar mejor la historia de desarrollo; hoy existe solo commit base

## Prioridad real si tiempo aprieta

### P0 - No negociar

- Fase 0 completa
- Escenarios BDD base creados y mantenidos al dia
- Dominio completo con tests
- Commands/queries/handlers
- ValidationBehavior + validators
- CachingBehavior funcional
- EF Core + SQL Server + repositorios + seeder
- Dockerfile + docker compose funcionales
- Middleware de Problem Details
- Pagina de listado + crear + editar
- `dotnet test` y `docker compose up --build`
- README con decisiones

### P1 - Muy valioso

- Idempotencia persistida
- Interceptor de queries lentas
- Tests de arquitectura
- Test de integracion de eventos vs `SaveChanges`
- Error boundary consumiendo `ProblemDetails`

### P2 - Extra senior-plus

- Outbox Pattern completo
- Memento muy pulido para rollback compuesto
- Mejoras fuertes contra cache stampede mas alla de `SemaphoreSlim`

## Secuencia sugerida de commits

1. `chore: scaffold clean architecture solution`
2. `test: add bdd feature scenarios and traceability`
3. `feat: add domain primitives and custom exceptions`
4. `feat: implement money and sku value objects`
5. `feat: implement product aggregate and domain events`
6. `test: cover domain invariants and value objects`
7. `feat: add cqrs contracts and handlers`
8. `feat: add validation logging and caching behaviors`
9. `feat: implement sql server persistence and repositories`
10. `chore: add docker compose and container runtime`
11. `feat: add controller endpoints and problem details middleware`
12. `feat: build blazor product management pages`
13. `test: add integration and architecture tests`
14. `docs: add readme adrs and api examples`

## Riesgos que debemos vigilar

- Duplicar reglas entre FluentValidation y dominio.
- Publicar eventos antes de confirmar `SaveChanges`.
- Meter logica de negocio dentro de handlers.
- Acoplar Blazor directo a EF en vez de pasar por MediatR/API.
- Saltar feature BDD y codificar sin escenario.
- Dejar UI primero y dominio para despues.
- Descubrir tarde fallos de arranque entre app y SQL Server en Docker.
- Guardar secretos o passwords dentro de archivos versionados.
- Olvidar documentar trade-offs no implementados.

## Definition of Done final

- `dotnet test` pasa completo.
- `docker compose up --build` levanta app + SQL Server sin pasos manuales raros.
- SQL Server vive en contenedor con volumen persistente.
- Seeder carga datos si base esta vacia.
- Listado, alta, edicion y borrado funcionan.
- Invariantes se protegen desde dominio.
- Errores devuelven `ProblemDetails`.
- README explica arranque, decisiones y pendientes.
- Historial Git cuenta historia de desarrollo.
