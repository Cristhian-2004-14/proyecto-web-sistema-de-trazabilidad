# Análisis, comparación y plan — Palma Verde

## 1. Proyecto base

La carpeta obligatoria identificada es `POS/`, con la solución `POS.sln`. Usa .NET 9 y una arquitectura cliente-servidor de tres proyectos:

- `POS.Shared`: entidades, enumeraciones y DTOs compartidos.
- `POS.Server`: ASP.NET Core Web API, `AppDbContext`, repositorios, servicios, controladores, JWT y migraciones EF Core.
- `POS.Client`: Blazor WebAssembly, páginas Razor, layouts y recursos web.

La persistencia usa Entity Framework Core 9.0.19 y SQL Server mediante `DefaultConnection`. El patrón CRUD existente se infirió de `UserRepository`: interfaz + implementación con `AppDbContext`, servicio para lógica de autenticación y API para el cliente. La base no traía controladores funcionales ni páginas CRUD; se completaron sin crear capas nuevas.

Archivos de referencia: `Entities/User.cs`, `DTOs/LoginRequest.cs`, `Repositories/UserRepository.cs`, `Services/AuthService.cs`, `Data/AppDbContext.cs`, `Migrations/*InitialDB*`, `Layout/MainLayout.razor` y `Pages/Home.razor`.

## 2. Mapeo completo del modelo obligatorio

| Entidad | Ubicación prevista | Relaciones | CRUD/reglas principales |
|---|---|---|---|
| ROL | Shared entidad; Server repositorio/controlador; Client página | 1:N USUARIO | CRUD administrativo; nombre único y baja lógica mediante `IsActive` |
| USUARIO | Shared entidad/DTO; Server repositorio/servicio/controlador; Client página | N:1 ROL; luego 1:N con RECEPCION, LOTE, DESPACHO y MOVIMIENTO | CRUD, activar/desactivar, usuario/email únicos, BCrypt, autorización |
| PROVEEDOR | Shared; Server repositorio/controlador; Client página | 1:N RECEPCION | CRUD con baja lógica |
| MATERIA_PRIMA | Shared; Server repositorio/controlador; Client página | 1:N DETALLE_RECEPCION y DETALLE_LOTE | CRUD, stock actual/mínimo >= 0 |
| RECEPCION | Sprint 2: Shared/Server/Client | N:1 PROVEEDOR y USUARIO; 1:N DETALLE_RECEPCION | CRUD y confirmación transaccional; fecha/usuario |
| DETALLE_RECEPCION | Sprint 2: Shared/Server | N:1 RECEPCION y MATERIA_PRIMA | cantidad > 0; incrementa stock al confirmar |
| PRODUCTO | Sprint 2: adaptar entidad base Shared; Server/Client | 1:N LOTE, DETALLE_DESPACHO y MOVIMIENTO | CRUD; stock no negativo |
| LOTE_PRODUCCION | Sprint 3: Shared/Server/Client | N:1 PRODUCTO y USUARIO; 1:N DETALLE_LOTE | código único, cantidades válidas, estados |
| DETALLE_LOTE_MATERIA_PRIMA | Sprint 3: Shared/Server | N:1 LOTE y MATERIA_PRIMA | cantidad > 0, disponibilidad, transacción |
| DESPACHO | Sprint 4: Shared/Server/Client | N:1 USUARIO; 1:N DETALLE_DESPACHO | destino requerido, confirmación transaccional |
| DETALLE_DESPACHO | Sprint 4: Shared/Server | N:1 DESPACHO y PRODUCTO | cantidad > 0 y <= stock |
| MOVIMIENTO_INVENTARIO | Sprint 4: adaptar entidad base `Stock` | N:1 PRODUCTO y USUARIO | consulta; fecha, usuario, tipo y referencia obligatorios |

## 3. Plan concreto de Sprint 1

Archivos creados:

- Shared: `RoleEntity.cs`, `Supplier.cs`, `RawMaterial.cs`, `UserUpdateRequest.cs`.
- Server: controladores de autenticación y catálogos, repositorio genérico de catálogos, `DbSeeder.cs` y migración `PalmaVerdeSprint1`.
- Client: proveedor de autenticación, handler JWT, login, panel y páginas CRUD.
- Pruebas: `POS.Tests.csproj` y `AuthServiceTests.cs`.
- Documentación: `README.md` y este informe.

Archivos modificados:

- `User.cs`, DTOs de acceso, `AppDbContext.cs`, repositorio/servicio de usuario, `Program.cs`, configuraciones, proyectos, router, layout, menú, estilos e índice web.
- La primera migración duplicada del proyecto base se dejó como no-op (sin eliminarla) para que una base nueva no intente crear dos veces las mismas tablas.

Migración/configuración:

- Tablas Sprint 1: `ROL`, `USUARIO`, `PROVEEDOR`, `MATERIA_PRIMA`.
- Índices únicos en rol, usuario y correo.
- FK `USUARIO.RoleId → ROL.Id` con borrado restringido.
- Checks SQL para stocks no negativos.
- JWT, CORS, SQL Server y datos demo configurados en Server; URL API en Client.

### Cierre de auditoría de Sprint 1

- `GET /api/users` y `GET /api/users/{id}` devuelven `UserResponse`, sin contraseña ni `PasswordHash`.
- `ROL` incorpora `IsActive`; puede activarse o desactivarse sin eliminación física.
- Los roles inactivos no aparecen al crear usuarios y el backend rechaza su asignación.
- Un usuario con rol inactivo conserva su registro, pero no puede iniciar una sesión nueva.
- La migración incremental `RoleLogicalDeletion` agrega el estado con valor inicial activo para preservar roles existentes.

## 4. Decisiones de adaptación

Se reemplazó el enum de rol por `RoleEntity` porque el modelo obligatorio exige tabla `ROL` y relación 1:N. Se mantuvieron inglés y propiedades PascalCase, convención del código docente, mientras los nombres físicos de las cuatro tablas obligatorias usan el modelo académico. Los módulos POS heredados permanecen en el código para no eliminar trabajo del proyecto base y se adaptarán solamente cuando corresponda por Sprint.
