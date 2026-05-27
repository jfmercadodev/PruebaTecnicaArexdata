# ADR-002: Publicar eventos de dominio despues del commit

- Estado: Aceptado
- Date: 2026-05-26

## Contexto

El agregado `Product` emite eventos de negocio como `ProductCreated` y `ProductUpdated`. Publicarlos antes de completar la persistencia arriesga efectos secundarios sobre datos que nunca terminaron comprometidos.

## Decision

Mantener los eventos de dominio pendientes dentro del agregado y publicarlos solo despues de `SaveChanges` exitoso o de completar la unidad de trabajo.

## Consecuencias

- El flujo de eventos refleja el estado realmente comprometido.
- Un test de integracion puede verificar que no hay publicacion si la persistencia falla.
- Es mas simple que un outbox completo y aun asi protege la consistencia dentro del mismo proceso.

## Alternativas consideradas

- Publicar dentro de los metodos del agregado: descartado porque la persistencia aun podria fallar despues.
- Patron Outbox completo: mas fuerte para integraciones distribuidas, pero demasiado grande para el alcance de este ejercicio.
