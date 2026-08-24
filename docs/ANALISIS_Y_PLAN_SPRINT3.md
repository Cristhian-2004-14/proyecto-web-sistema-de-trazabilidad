# Análisis y cierre de Sprint 3 — Producción por lotes

## Objetivo

Implementar PB08 (lote de producción), PB09 (consumo de materia prima) y PB10 (producto terminado), conservando .NET 9, EF Core 9.0.19, SQL Server LocalDB, JWT, BCrypt y la estructura `POS.Client / POS.Server / POS.Shared / POS.Tests`.

## Arquitectura reutilizada

- Entidades y DTO públicos en `POS.Shared`.
- Fluent API, migraciones y persistencia en `POS.Server`.
- Servicio Scoped específico para las operaciones transaccionales.
- Controlador REST con autorización por roles.
- Blazor WebAssembly con `HttpClient` autenticado y visibilidad de acciones por rol.
- xUnit y SQLite en memoria para validar reglas y rollback sin alterar LocalDB.

## Entidades y relaciones

`ProductionLot`, mapeado a `LOTE_PRODUCCION`, contiene producto, usuario responsable, código único, fechas, cantidades planificada/producida y estado. Se relaciona N:1 con `InventoryProduct` (`PRODUCTO`), N:1 con `User` (`USUARIO`) y 1:N con sus detalles.

`ProductionLotMaterialDetail`, mapeado a `DETALLE_LOTE_MATERIA_PRIMA`, relaciona el lote con `RawMaterial` (`MATERIA_PRIMA`) y registra `QuantityUsed`. El índice único `(ProductionLotId, RawMaterialId)` impide duplicados. Las cantidades tienen checks SQL positivos; la cantidad producida permite cero mientras el lote no finalizó.

Estados: `Pendiente`, `EnProceso` y `Finalizado`. Solo se permiten las transiciones Pendiente → EnProceso → Finalizado.

## DTO

- `CreateProductionLotRequest`
- `MaterialConsumptionRequest`
- `MaterialConsumptionItemRequest`
- `FinishProductionLotRequest`
- `ProductionLotResponse`
- `ProductionLotMaterialResponse`

El usuario responsable se obtiene exclusivamente del claim `NameIdentifier` del JWT y se valida contra usuario y rol activos; no se acepta un `UserId` del navegador.

## Endpoints y autorización

| Método | Ruta | Roles |
|---|---|---|
| GET | `/api/production-lots` | Administrador, Almacén, Producción, Gerencia |
| GET | `/api/production-lots/{id}` | Administrador, Almacén, Producción, Gerencia |
| POST | `/api/production-lots` | Administrador, Producción |
| POST | `/api/production-lots/{id}/consume` | Administrador, Producción |
| POST | `/api/production-lots/{id}/finish` | Administrador, Producción |

Almacén y Gerencia pueden consultar pero reciben HTTP 403 al intentar modificar producción. Blazor además oculta formularios y botones que no corresponden al rol.

## Flujo de creación

Valida usuario autenticado, producto existente y activo, código obligatorio y único, fecha válida y cantidad planificada mayor a cero. Persiste el lote en estado Pendiente con cantidad producida cero. No altera inventarios.

## Consumo y transacción

Solo un lote Pendiente admite consumo. Se validan lista no vacía, materiales sin duplicar, existentes, activos, cantidades positivas y stock suficiente. Dentro de una transacción se insertan los detalles, se descuentan existencias y se cambia el lote a EnProceso. Cualquier excepción ejecuta rollback completo. Una repetición sobre EnProceso o Finalizado devuelve el estado actual sin insertar detalles ni volver a descontar, garantizando idempotencia.

## Finalización y transacción

Solo un lote EnProceso puede finalizarse y la cantidad producida debe ser positiva; puede diferir de la planificada. Dentro de una transacción se registran cantidad, fecha final y estado Finalizado, y se incrementa `PRODUCTO.CurrentStock`. Si falla cualquier guardado se revierte todo. Una petición repetida sobre Finalizado devuelve el lote sin incrementar nuevamente el producto.

## Interfaz Blazor

- `ProductionLots.razor`: listado con código, producto, fecha, cantidades, estado y responsable.
- `NewProductionLot.razor`: alta de lote con productos activos.
- `ProductionLotDetail.razor`: trazabilidad básica, constructor de consumo con stock disponible y formulario de finalización según estado.

## Migración

Migración única `20260821191807_Sprint3ProductionLots`, EF Core 9.0.19. Crea exclusivamente `LOTE_PRODUCCION`, `DETALLE_LOTE_MATERIA_PRIMA`, índices, checks y claves foráneas hacia `PRODUCTO`, `USUARIO` y `MATERIA_PRIMA`. Fue aplicada correctamente a `PalmaVerdeDb`; no modifica migraciones previas.

## Pruebas y validación

Se conservaron las 17 pruebas anteriores y se agregaron 16: creación válida, código duplicado, plan no positivo, producto inactivo, consumo válido, descuento, stock insuficiente, material inactivo, rollback de consumo, doble consumo, finalización, aumento de producto, doble finalización, rollback de finalización y restricciones para Almacén/Gerencia.

Resultado: **33 superadas, 0 fallidas, 0 omitidas**. Compilación: **0 errores, 0 advertencias**.

Validación real en SQL Server: lote `LP-MANUAL-20260821152006` (#1), Pendiente → EnProceso → Finalizado; materia prima 605 → 604; producto 0 → 2; segunda finalización mantuvo producto en 2; modificación como Gerencia devolvió HTTP 403.

## Archivos

Se agregaron las dos entidades, DTO, interfaz/servicio, controlador, tres páginas Razor, prueba Sprint 3, migración y este documento. Se modificaron las tres entidades relacionadas, `AppDbContext`, `Program.cs`, navegación, estilos, snapshot EF y README.

## Prueba manual

1. Ejecutar `dotnet run --project POS/POS.Server --launch-profile http`.
2. Ejecutar `dotnet run --project POS/POS.Client --launch-profile http`.
3. Abrir `http://localhost:5010` e iniciar como `produccion / PalmaVerde2026!`.
4. Crear un lote desde Producción y confirmar que queda Pendiente sin cambiar stock.
5. Abrirlo, agregar materiales dentro del stock disponible y confirmar consumo.
6. Revisar Inventario: las materias primas deben disminuir exactamente una vez.
7. Finalizar con una cantidad positiva y revisar Productos: el stock debe aumentar.
8. Repetir la petición de finalización: el stock debe permanecer igual.
9. Iniciar como `gerencia / PalmaVerde2026!`: el listado y detalle son visibles, pero no las acciones; una petición POST manual debe responder 403.

## Pendientes deliberados

No se implementaron despachos, detalles de despacho, movimientos de inventario, trazabilidad global ni reportes. Corresponden exclusivamente a Sprint 4.
