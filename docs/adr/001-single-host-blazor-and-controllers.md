# ADR-001: Host unico con Blazor Server y Controllers

- Estado: Aceptado
- Date: 2026-05-26

## Contexto

La prueba tecnica exige una interfaz interactiva y tambien un contrato HTTP con comportamiento completo de controllers y `ProblemDetails`.

## Decision

Usar un unico host ASP.NET Core, `ProductCatalog.Web`, que contenga:

- `Blazor Server` UI
- MVC `Controllers`
- grafo de DI compartido para aplicacion e infraestructura

## Consecuencias

- Despliegue y ejecucion local mas simples.
- Logging, correlation id, manejo de excepciones y ruta de arranque/migracion compartidos.
- La UI puede reutilizar servicios de aplicacion cuando convenga, mientras el contrato HTTP publico sigue disponible.

## Alternativas consideradas

- API separada + host Blazor separado: fronteras de despliegue mas limpias, pero mas boilerplate y mas piezas operativas para este ejercicio.
- Minimal APIs: descartado porque el requerimiento pide explicitamente controllers completos.
