# Sistema de Trazabilidad Logística y Gestión de Inventarios para Palma Verde S.A.

Prototipo académico web para centralizar autenticación, catálogos, recepción, producción por lotes, inventarios, despachos, trazabilidad y reportes de Palma Verde S.A.

## Objetivo

Controlar de forma segura y demostrable el ciclo interno desde la recepción de materias primas hasta la producción y despacho del producto terminado, manteniendo existencias, responsables, movimientos e información básica de trazabilidad.

## Tecnologías y arquitectura

- .NET 9 y C#.
- Blazor WebAssembly en `POS.Client`.
- ASP.NET Core Web API en `POS.Server`.
- Entidades y DTOs compartidos en `POS.Shared`.
- Entity Framework Core 9 con SQL Server.
- JWT para autenticación y autorización por roles.
- BCrypt para hash de contraseñas.
- Bootstrap y CSS propio responsivo.

La implementación conserva la solución y las carpetas del proyecto base del docente. El flujo es `Client → Controllers → Services/Repositories → AppDbContext → SQL Server`.

- `POS.Client`: interfaz Blazor WebAssembly, navegación, formularios y consumo autenticado de la API.
- `POS.Server`: API, autorización, reglas de negocio, transacciones, repositorios, EF Core y seeder.
- `POS.Shared`: entidades y contratos DTO compartidos, sin depender de Client ni Server.
- `POS.Tests`: pruebas xUnit de seguridad, negocio, persistencia, rollback y permisos.

Las dependencias mantienen la dirección esperada: Client y Server referencian Shared; Tests referencia Server y Shared; Shared no referencia las otras capas.

## Requisitos previos

- SDK y runtime de .NET 9.
- SQL Server Express/Developer o LocalDB.
- Puertos libres `5216` (API) y `5010` (cliente).

## Configuración y ejecución

1. La configuración demo usa `Server=(localdb)\\PalmaVerdeLocalDB;Database=PalmaVerdeDb;Trusted_Connection=True;TrustServerCertificate=True`. Para otra instancia, cambiar `DefaultConnection` en `POS/POS.Server/appsettings.json`.
2. Configurar una clave JWT privada mediante User Secrets. El valor queda en el perfil local y no se versiona en Git:

   ```powershell
   dotnet user-secrets set "JwtSettings:SecretKey" "REEMPLAZAR-POR-UNA-CLAVE-ALEATORIA-DE-AL-MENOS-32-CARACTERES" --project "POS/POS.Server/POS.Server.csproj"
   dotnet user-secrets list --project "POS/POS.Server/POS.Server.csproj"
   ```

3. Restaurar y compilar:

   ```powershell
   dotnet restore POS\POS.sln
   dotnet build POS\POS.sln
   ```

4. Crear/actualizar la base manualmente si fuera necesario:

   ```powershell
   dotnet ef database update --project POS\POS.Server\POS.Server.csproj --startup-project POS\POS.Server\POS.Server.csproj
   ```

5. Iniciar la API (también aplica migraciones pendientes y carga datos demo):

   ```powershell
   dotnet run --project POS\POS.Server --launch-profile http
   ```

6. En otra terminal iniciar el cliente:

   ```powershell
   dotnet run --project POS\POS.Client --launch-profile http
   ```

7. Abrir `http://localhost:5010`.

La URL de la API del cliente se configura en `POS/POS.Client/wwwroot/appsettings.json`.

## Usuarios demo (solo desarrollo)

Todos usan la contraseña `PalmaVerde2026!`:

| Usuario | Rol |
|---|---|
| `admin` | Administrador |
| `produccion` | Producción |
| `almacen` | Almacén |
| `gerencia` | Gerencia |

Las contraseñas se almacenan exclusivamente como hash BCrypt.

## Roles

- Administrador: usuarios, roles, catálogos, supervisión, producción, despachos y reportes autorizados.
- Producción: lotes, consumo, producto terminado, inventario, movimientos, trazabilidad y reporte de producción.
- Almacén: recepciones, inventario, despachos, movimientos, trazabilidad y reportes autorizados.
- Gerencia: consultas de inventario, lotes, despachos, movimientos, trazabilidad y reportes; sin operaciones de modificación.

## Estado de Sprints

- Sprint 1: terminado. Login/logout, JWT, roles con baja lógica, usuarios, proveedores, materias primas, validaciones, datos demo y pruebas básicas. Las respuestas públicas de usuarios usan DTOs y nunca incluyen `PasswordHash`.
- Sprint 2: terminado. Recepciones transaccionales, detalle de recepción, incremento idempotente de stock, consulta de inventario con búsqueda/alerta de stock bajo y CRUD de productos con baja lógica.
- Sprint 3: terminado. Lotes de producción, consumo transaccional de materia prima, registro transaccional de producto terminado, estados e idempotencia.
- Sprint 4: terminado. Despachos transaccionales, movimientos de inventario, trazabilidad de lotes y reportes básicos.

Los cuatro Sprints planificados están implementados y validados.

## Pruebas

```powershell
dotnet test POS\POS.Tests\POS.Tests.csproj
```

Las pruebas cubren login válido, contraseña incorrecta, cuenta inactiva, rol inactivo, rechazo de asignación de roles inactivos, hash seguro al registrar y ausencia de `PasswordHash` en la respuesta pública de usuarios.

Sprint 2 agrega pruebas de recepción e inventario. Sprint 3 agrega pruebas de lotes, consumo, finalización e idempotencia. Sprint 4 agrega 23 pruebas de despachos, movimientos, trazabilidad, reportes, rollback y permisos. El total actual es **56 pruebas**.

## Sprint 4

- Despachos: `GET/POST /api/dispatches`, `GET /api/dispatches/{id}` y `PATCH /api/dispatches/{id}/confirm`.
- Movimientos: `GET /api/inventory/movements` con filtros por producto, tipo, fechas y usuario.
- Trazabilidad: `GET /api/traceability/lots/{code}`.
- Reportes: `/api/reports/production`, `/api/reports/inventory` y `/api/reports/dispatches`.

Un despacho confirmado registra cabecera, detalles, descuento de producto y movimientos de salida dentro de una transacción. Finalizar producción registra el movimiento `EntradaProduccion` en la misma transacción del lote. Las repeticiones no duplican stock ni movimientos.

## API y prueba manual de Sprint 3

- `GET/POST /api/production-lots`
- `GET /api/production-lots/{id}`
- `POST /api/production-lots/{id}/consume`
- `POST /api/production-lots/{id}/finish`

Iniciar sesión como `produccion`, abrir **Producción → Nuevo lote**, seleccionar un producto activo y registrar código, fecha y cantidad planificada. En el detalle, agregar materias primas sin superar su stock y confirmar el consumo; el inventario de materia prima disminuye en una transacción. Después ingresar la cantidad producida y finalizar; el stock del producto aumenta una sola vez. Administrador puede ejecutar las mismas operaciones; Almacén y Gerencia solo consultan.

## API y prueba manual de Sprint 2

- `GET/POST /api/receptions`
- `GET /api/receptions/{id}`
- `PATCH /api/receptions/{id}/confirm`
- `GET /api/inventory/raw-materials`
- `GET/POST /api/products`
- `GET/PUT /api/products/{id}`
- `PATCH /api/products/{id}/status`

Para probar, iniciar sesión como `almacen`, abrir **Recepciones → Nueva recepción**, seleccionar proveedor y materias primas, agregar cantidades mayores que cero y registrar. Luego comprobar el ingreso en **Recepciones** y el aumento exacto en **Inventario**. `admin` puede además gestionar productos. Producción y Gerencia disponen de consulta, pero el backend rechaza el registro de recepciones.

## Limitaciones conocidas

- El prototipo requiere una instancia SQL Server disponible.
- Los valores de desarrollo de conexión/JWT deben sustituirse para despliegues reales.
- La entidad `MOVIMIENTO_INVENTARIO` se mantendrá vinculada a `PRODUCTO`, tal como exige el modelo académico.
- La anulación de recepciones confirmadas no forma parte de Sprint 2; se evita implementar una reversión parcial. Se abordará únicamente cuando el flujo histórico esté definido.
- Cada lote admite un único consumo confirmado. Una solicitud repetida devuelve el lote sin volver a descontar stock; un lote finalizado tampoco vuelve a incrementar producto.
- La trazabilidad implementada identifica lote, producto, materias primas consumidas y movimientos del producto terminado. El modelo no vincula cada consumo con una recepción específica, por lo que no se afirma trazabilidad exacta proveedor → recepción → lote.
- La exportación CSV/PDF/Excel queda como mejora futura; los reportes priorizan visualización en pantalla.

El análisis del proyecto base, el mapeo de entidades y el plan de archivos están en `docs/ANALISIS_Y_PLAN_SPRINT1.md`.
