# ADR-003: Usar SQL Server en Docker para ejecucion local

- Estado: Aceptado
- Date: 2026-05-26

## Contexto

El requerimiento pide SQL Server y una solucion completamente dockerizada. La configuracion local debe ser reproducible y parecida a la forma esperada de ejecucion.

## Decision

Usar un contenedor `SQL Server 2022 Developer` en `docker-compose.yml`, junto con:

- persistent Docker volume
- healthcheck before web startup
- environment-driven connection string
- automatic migration + seed on application startup

## Consecuencias

- Entorno local consistente para quienes revisen la solucion.
- Menos diferencias especificas de maquina que con instaladores locales de base de datos.
- La entrega depende de la disponibilidad de Docker para los smoke tests finales.

## Alternativas consideradas

- SQLite: descartado porque el requerimiento cambio a SQL Server.
- Solo SQL Server instalado en la maquina local: viable, pero menos reproducible que una configuracion con contenedores.
