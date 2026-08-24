# Análisis y cierre de Sprint 2 — Recepción e inventario

## 1. Objetivo

Implementar PB05 (registrar recepción), PB06 (consultar inventario de materia prima) y PB07 (gestión de productos), conservando .NET 9, SQL Server, EF Core 9.0.19, JWT, BCrypt y la separación `POS.Client / POS.Server / POS.Shared / POS.Tests`.

## 2. Análisis de la arquitectura conservada

- Entidades y DTO: `POS.Shared`, con DataAnnotations y propiedades PascalCase.
- Persistencia: `AppDbContext` y Fluent API en `POS.Server`.
- Catálogos simples: repositorio genérico `ICatalogRepository<T>`.
- Operaciones de negocio: servicios Scoped; recepción usa un servicio específico por requerir transacción y varias entidades.
- API: controladores REST con autorización por rol.
- Interfaz: páginas Razor que consumen la API con `HttpClient` autenticado.
- Pruebas: xUnit; Sprint 2 usa SQLite en memoria para comprobar transacciones reales sin afectar SQL Server local.

## 3. Entidades y relaciones

### RECEPCION

- `Id`, `SupplierId`, `UserId`, `Date`, `Observation`, `Status`.
- N:1 con `PROVEEDOR` y N:1 con `USUARIO`.
- 1:N con `DETALLE_RECEPCION`.
- Estados implementados: `Pendiente`, `Confirmada` y constante `Anulada`. La creación acepta Pendiente o Confirmada; la anulación operativa queda fuera del alcance.

### DETALLE_RECEPCION

- `Id`, `ReceptionId`, `RawMaterialId`, `Quantity` decimal(18,2).
- N:1 con `RECEPCION` y N:1 con `MATERIA_PRIMA`.
- Check SQL `Quantity > 0` e índice único `(ReceptionId, RawMaterialId)`.

### PRODUCTO

- Implementado mediante `InventoryProduct`, mapeado físicamente a `PRODUCTO`, para no romper la entidad POS heredada llamada `Product` ni sus tablas de la solución docente.
- Nombre, descripción, unidad de medida, stock actual/mínimo decimal(18,2) y estado.
- Checks SQL para impedir stocks negativos y baja lógica mediante `IsActive`.

## 4. DTO creados

- `CreateReceptionRequest`
- `ReceptionDetailRequest`
- `ReceptionResponse`
- `ReceptionDetailResponse`
- `ProductRequest`
- `ProductResponse`
- `ProductStatusRequest`
- `RawMaterialInventoryResponse`

El usuario responsable no se recibe desde el cliente: se obtiene del claim `NameIdentifier` del JWT y se valida nuevamente contra `USUARIO` y su rol activo.

## 5. Endpoints

| Método | Ruta | Roles |
|---|---|---|
| GET | `/api/receptions` | Administrador, Almacén, Producción, Gerencia |
| GET | `/api/receptions/{id}` | Administrador, Almacén, Producción, Gerencia |
| POST | `/api/receptions` | Administrador, Almacén |
| PATCH | `/api/receptions/{id}/confirm` | Administrador, Almacén |
| GET | `/api/inventory/raw-materials` | Administrador, Almacén, Producción, Gerencia |
| GET | `/api/products`, `/api/products/{id}` | Todos los roles autenticados del proyecto |
| POST/PUT/PATCH | `/api/products...` | Administrador |

## 6. Reglas de recepción y transacción

`ReceptionService.CreateAsync` valida proveedor, usuario autenticado, estado, detalles, materias primas activas, cantidades positivas y ausencia de duplicados. Después abre una transacción EF Core:

1. Inserta `RECEPCION` y `DETALLE_RECEPCION`.
2. Ejecuta `SaveChangesAsync` dentro de la transacción.
3. Si el estado es Confirmada, incrementa cada `RawMaterial.CurrentStock`.
4. Ejecuta el segundo `SaveChangesAsync` dentro de la misma transacción.
5. Confirma con `CommitAsync`.

Ante cualquier excepción se ejecuta `RollbackAsync`. La prueba `FailureDuringStockUpdate_RollsBackReceptionAndDetails` fuerza un fallo en el segundo guardado y confirma que no quedan recepción, detalles ni stock modificado.

`ConfirmAsync` solo cambia `Pendiente → Confirmada`. Si la recepción ya está confirmada, devuelve el resultado sin modificar stock; esto garantiza idempotencia. No se implementó anulación porque la reversión histórica no pertenece al alcance definido.

## 7. Inventario

La página `/inventory` es de consulta y no permite editar stock. Muestra nombre, unidad, stock actual, mínimo, estado, búsqueda por nombre, filtro local por estado y la alerta `STOCK BAJO` cuando `CurrentStock <= MinimumStock`.

## 8. Interfaz Blazor

- `Receptions.razor`: listado.
- `NewReception.razor`: proveedor, fecha, detalle múltiple, cantidades y observación.
- `ReceptionDetailPage.razor`: cabecera, responsable, estado y detalles.
- `Inventory.razor`: búsqueda, filtro y alertas.
- `Products.razor`: listado/búsqueda para consulta y CRUD administrativo.
- `NavMenu.razor` y `Home.razor`: accesos visibles según roles.

## 9. Migración

Migración única: `20260821190217_Sprint2ReceptionInventory`.

Crea exclusivamente `RECEPCION`, `DETALLE_RECEPCION` y `PRODUCTO`, sus restricciones, índices y relaciones. No modifica ni elimina migraciones previas.

## 10. Datos demo

Se conserva todo Sprint 1 y se agrega un producto mínimo:

- Palmito entero; unidad; stock actual 0; stock mínimo 100; activo.

Durante la validación manual se registró la recepción demo #1 por el usuario `almacen`, con 5 unidades de Envase 1 litro.

## 11. Pruebas

Se conservan las 7 pruebas de Sprint 1 y se agregan 10:

1. Recepción válida.
2. Incremento de stock.
3. Cantidad no positiva rechazada.
4. Proveedor inactivo rechazado.
5. Materia prima inactiva rechazada.
6. Rollback ante fallo durante actualización.
7. Confirmación repetida sin doble incremento.
8. Producto válido.
9. Producto con stock negativo rechazado.
10. POST de recepción limitado a Administrador y Almacén.

Resultado final: **17 superadas, 0 fallidas, 0 omitidas**.

## 12. Validación real

- Compilación: 0 errores, 0 advertencias.
- SQL Server: migración aplicada y producto demo sembrado.
- Recepción #1: stock 600 → 605.
- Segunda confirmación: stock permanece 605.
- Intento de Gerencia de registrar recepción: HTTP 403.
- Las cinco páginas de Sprint 2 cargan correctamente en Blazor.

## 13. Archivos principales modificados

- Shared: entidades existentes relacionadas, `Reception`, `ReceptionDetail`, `InventoryProduct` y DTO de Sprint 2.
- Server: `AppDbContext`, `DbSeeder`, `Program`, `ReceptionService` y controladores de recepción/inventario/productos.
- Client: navegación, panel, estilos y cinco páginas Razor.
- Tests: referencia SQLite y `Sprint2Tests.cs`.
- Documentación: `README.md` y este documento.

## 14. Pendientes deliberados

- Anulación transaccional de recepciones confirmadas.
- Producción, lotes, producto terminado, despachos, movimientos, trazabilidad y reportes.
- No comenzar Sprint 3 hasta validar Sprint 2 con el usuario.
