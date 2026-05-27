# Trazabilidad BDD

## Objetivo

Mapear escenarios de comportamiento contra tareas de implementacion. Los archivos `.feature` guian la secuencia de trabajo y el plan registra el estado de avance.

## Escenarios por area

| ID Escenario | Feature | Tareas principales | Estado |
| --- | --- | --- | --- |
| `BDD-001` | Crear producto valido | `F1-03`, `F1-04`, `F1-05`, `F1-08`, `F2-03`, `F2-07` | Specs ejecutables + dominio/aplicacion cubiertos |
| `BDD-002` | Rechazar precio de venta menor al costo | `F1-04`, `F1-05`, `F1-15`, `F2-17` | Specs ejecutables + dominio cubierto |
| `BDD-003` | Rechazar SKU duplicado | `F2-13`, `F2-14`, `F4-07` | Specs ejecutables + path de controller cubierto |
| `BDD-004` | Reusar respuesta idempotente | `F2-05`, `F2-15`, `F3-11` | Specs ejecutables + aplicacion cubierta |
| `BDD-005` | Actualizar precio y stock correctamente | `F1-05`, `F1-06`, `F1-07`, `F1-08`, `F2-07` | Dominio + aplicacion cubiertos |
| `BDD-006` | Revertir actualizacion compuesta invalida | `F1-10`, `F2-17` | Dominio + aplicacion cubiertos |
| `BDD-007` | Rechazar stock por debajo de cero | `F1-07`, `F1-17` | Dominio cubierto |
| `BDD-008` | Listar productos con paginacion y ordenamiento | `F3-08`, `F3-10`, `F4-07`, `F4-11` | Specs ejecutables + infraestructura/controller/UI cubiertos |
| `BDD-009` | Filtrar productos por nombre o SKU | `F3-10`, `F4-07`, `F4-11` | Specs ejecutables + infraestructura/controller/UI cubiertos |
| `BDD-010` | Mostrar metadata de origen de cache | `F2-08`, `F2-11`, `F2-12`, `F4-07`, `F4-12` | Specs ejecutables + payload de controller/UI cubiertos |
| `BDD-011` | Invalidar cache despues de escritura | `F2-11`, `F3-13` | Specs ejecutables + aplicacion/infraestructura cubiertas |
| `BDD-012` | Retornar 422 por violacion de invariante de dominio | `F4-05`, `F4-07` | Specs ejecutables + web cubierta |
| `BDD-013` | Retornar 404 para producto inexistente | `F4-05`, `F4-07` | Specs ejecutables + web cubierta |
| `BDD-014` | Retornar 400 por error de validacion | `F2-13`, `F4-05`, `F4-07` | Specs ejecutables + web cubierta |
| `BDD-015` | Exponer endpoints con controllers sin Minimal APIs | `F4-01`, `F4-07` | Web cubierta |
| `BDD-016` | Arrancar aplicacion contra contenedor SQL Server | `F0-07`, `F0-08`, `F3-17`, `F3-18`, `F3-19`, `F4-06` | Docker + migraciones + smoke cubiertos |
| `BDD-017` | Sembrar catalogo cuando la base esta vacia | `F3-16`, `F5-13` | Specs ejecutables + infraestructura cubiertas |
| `BDD-018` | Evitar seeding duplicado en reinicio | `F3-16`, `F5-14` | Specs ejecutables + infraestructura cubiertas |
| `BDD-019` | Propagar correlation id en respuestas y errores | `F4-03`, `F4-04`, `F4-05` | Specs ejecutables + web cubierta |
| `BDD-020` | Renderizar pagina de catalogo con controles y metadata | `F4-08`, `F4-11`, `F4-12`, `F4-13` | Smoke de render UI cubierto |
| `BDD-021` | Bloquear formulario invalido hasta validar SKU | `F4-09`, `F4-14`, `F4-17`, `F4-18`, `F4-19` | Specs de componente ejecutables + UI cubierta |
| `BDD-022` | Renderizar formulario de edicion con snapshot original | `F4-10`, `F4-15`, `F4-17` | Smoke de render UI cubierto |
| `BDD-023` | Mostrar detalle tecnico en Development desde Error Boundary | `F4-20`, `F4-21` | Specs de componente ejecutables + UI cubierta |
| `BDD-024` | Mostrar mensaje amigable en Production desde Error Boundary | `F4-20`, `F4-21` | Specs de componente ejecutables + UI cubierta |
| `BDD-025` | Leer `title` de API en UI y exponer `detail` solo en Development | `F4-22` | Contrato compartido de `ProblemDetails` cubierto por servicio + tests de integracion |
