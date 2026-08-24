# Análisis y cierre de Sprint 4

## Objetivo y arquitectura

Se implementaron PB11–PB14 sobre .NET 9, Blazor WebAssembly, ASP.NET Core API, EF Core 9.0.19, SQL Server LocalDB, JWT y BCrypt, conservando `POS.Client`, `POS.Server`, `POS.Shared` y `POS.Tests`.

## Entidades y relaciones

- `Dispatch` → `DESPACHO`: N:1 con `USUARIO`, fecha, destino, observación y estado.
- `DispatchDetail` → `DETALLE_DESPACHO`: N:1 con despacho y producto, cantidad positiva e índice único despacho/producto.
- `InventoryMovement` → `MOVIMIENTO_INVENTARIO`: N:1 con producto y usuario, tipo, cantidad positiva, fecha y referencia.

Tipos: `EntradaProduccion` y `SalidaDespacho`. Las cantidades se almacenan positivas; la interfaz muestra signo según tipo.

## DTO y endpoints

DTO principales: `CreateDispatchRequest`, `DispatchDetailRequest`, `DispatchResponse`, `DispatchDetailResponse`, `InventoryMovementResponse`, `LotTraceabilityResponse`, `ProductionReportRow`, `InventoryReportRow` y `DispatchReportRow`.

| API | Autorización |
|---|---|
| GET despachos | todos los roles del proyecto |
| POST/PATCH despachos | Administrador, Almacén |
| GET movimientos | todos los roles |
| GET trazabilidad | todos los roles |
| reporte producción | Administrador, Producción, Gerencia |
| reporte inventario/despachos | Administrador, Almacén, Gerencia |

## Transacción e idempotencia

`DispatchService.CreateAsync` valida usuario/rol activos, destino, detalles, duplicados, productos activos, cantidades y stock. Dentro de una transacción inserta despacho/detalles, descuenta producto, crea un movimiento por detalle y confirma el estado. Cualquier fallo ejecuta rollback completo. Confirmar un despacho ya confirmado devuelve su estado sin descontar ni crear movimientos nuevamente.

`ProductionLotService.FinishAsync` crea ahora `EntradaProduccion` junto al incremento de producto, dentro de su transacción existente. El retorno anticipado para lotes finalizados evita un segundo incremento y un movimiento duplicado.

## Movimientos, trazabilidad y reportes

El historial se ordena por fecha descendente y filtra por producto, tipo, rango y usuario. La trazabilidad por código muestra lote, producto, responsable, fechas, cantidades, materias primas y movimientos cuya referencia es `LOTE:{codigo}`.

Limitación: `DETALLE_LOTE_MATERIA_PRIMA` no contiene una FK a `RECEPCION` o `DETALLE_RECEPCION`; por ello no puede determinarse qué recepción específica abasteció cada unidad consumida. La interfaz y documentación lo indican sin inventar relaciones.

Reportes en pantalla: producción por fechas; inventario de productos y materias primas con `STOCK BAJO`; despachos por fechas y detalle de producto. Exportación queda como mejora futura para evitar dependencias pesadas.

## Interfaz

Se crearon `Dispatches`, `NewDispatch`, `DispatchDetail`, `InventoryMovements`, `Traceability` y `Reports`, integradas al menú y protegidas con `Authorize`/`AuthorizeView`.

## Migración y pruebas

Migración única `20260821193420_Sprint4DistributionMovements`, aplicada sobre la base existente. Crea solamente `DESPACHO`, `DETALLE_DESPACHO`, `MOVIMIENTO_INVENTARIO`, checks, índices y relaciones; no modifica migraciones previas ni recrea `PRODUCTO`.

Se mantuvieron 33 pruebas y se agregaron 23. Resultado: **56 superadas, 0 fallidas, 0 omitidas**. Cobertura: validaciones, stock, rollback, idempotencia, movimientos, filtros, trazabilidad, autorización y reportes.

## Validación manual SQL Server

- Lote `LP-SPRINT4-20260821153620` finalizado con 10 unidades.
- Un movimiento `EntradaProduccion` con referencia al lote.
- Despacho #1 confirmado por Almacén, destino `Distribuidora Sprint 4`, cantidad 3.
- Un movimiento `SalidaDespacho`; stock final del producto: 9.
- Trazabilidad devolvió materia prima y movimiento.
- Reportes devolvieron producción, cuatro filas de inventario y despacho.
- POST de despacho con Gerencia devolvió HTTP 403.

## Prueba manual

Iniciar API y cliente, entrar como Producción, crear/consumir/finalizar un lote y revisar Movimientos. Entrar como Almacén, crear un despacho dentro del stock y verificar la salida. Consultar Trazabilidad por código y Reportes. Finalmente entrar como Gerencia: puede consultar, pero el backend debe devolver 403 al intentar registrar despacho.

## Observaciones

No quedan módulos posteriores solicitados. La trazabilidad recepción-específica y exportaciones son mejoras futuras explícitas, no datos simulados.
